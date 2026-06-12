using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Services;

public class BillingCycleService : IBillingCycleService
{
    private readonly SistemaIgrejaDbContext _context;
    private readonly BillingSettings _billing;
    private readonly IEmailService _emailService;
    private readonly ILogger<BillingCycleService> _logger;

    public BillingCycleService(
        SistemaIgrejaDbContext context,
        IOptions<BillingSettings> billing,
        IEmailService emailService,
        ILogger<BillingCycleService> logger)
    {
        _context = context;
        _billing = billing.Value;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<CicloBillingResultado> ExecutarTransicoesAutomaticasAsync(CancellationToken cancellationToken = default)
    {
        var resultado = new CicloBillingResultado();
        var agora = DateTime.UtcNow;
        var limiteCarencia = agora.AddDays(-_billing.CarenciaDias);

        var previous = _context.IgnoreTenantFilters;
        _context.IgnoreTenantFilters = true;
        try
        {
            // 0) Aviso de "trial acabando" (uma vez por assinatura).
            var limiteAviso = agora.AddDays(_billing.TrialAvisoDiasAntes);
            var trialsAcabando = await _context.Assinaturas
                .Where(a => a.Status == StatusAssinatura.Trial
                    && a.TrialAvisoEnviadoEm == null
                    && a.TrialFim != null && a.TrialFim > agora && a.TrialFim <= limiteAviso)
                .ToListAsync(cancellationToken);
            foreach (var assinatura in trialsAcabando)
            {
                assinatura.TrialAvisoEnviadoEm = agora;
                resultado.AvisosTrialEnviados++;
                await NotificarTrialAcabandoAsync(assinatura.TenantId, assinatura.TrialFim!.Value, cancellationToken);
            }

            // 1) Trial expirado → Inadimplente (inicia a carência).
            var trialsVencidos = await _context.Assinaturas
                .Where(a => a.Status == StatusAssinatura.Trial && a.TrialFim != null && a.TrialFim <= agora)
                .ToListAsync(cancellationToken);
            foreach (var assinatura in trialsVencidos)
            {
                assinatura.Status = StatusAssinatura.Inadimplente;
                assinatura.InadimplenteDesde = assinatura.TrialFim ?? agora;
                assinatura.DataAtualizacao = agora;
                resultado.TrialsExpirados++;
            }

            // 2) Inadimplente além da carência → Suspensa.
            var inadimplentes = await _context.Assinaturas
                .Where(a => a.Status == StatusAssinatura.Inadimplente
                    && a.InadimplenteDesde != null && a.InadimplenteDesde <= limiteCarencia)
                .ToListAsync(cancellationToken);
            foreach (var assinatura in inadimplentes)
            {
                assinatura.Status = StatusAssinatura.Suspensa;
                assinatura.SuspensaEm = agora;
                assinatura.DataAtualizacao = agora;
                resultado.Suspensos++;
                await NotificarSuspensaoAsync(assinatura.TenantId, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _context.IgnoreTenantFilters = previous;
        }

        if (resultado.TrialsExpirados > 0 || resultado.Suspensos > 0)
        {
            _logger.LogInformation("Ciclo de billing: {Trials} trials expirados, {Suspensos} suspensos.",
                resultado.TrialsExpirados, resultado.Suspensos);
        }

        return resultado;
    }

    private async Task NotificarTrialAcabandoAsync(int tenantId, DateTime trialFim, CancellationToken cancellationToken)
    {
        await EnviarParaAdminAsync(tenantId,
            "Seu período de teste está acabando",
            $"<p>Olá,</p><p>Seu período de teste termina em <strong>{trialFim:dd/MM/yyyy}</strong>. " +
            "Para continuar usando a plataforma sem interrupção, escolha a forma de pagamento na área de <strong>Assinatura</strong>.</p>",
            cancellationToken);
    }

    private async Task NotificarSuspensaoAsync(int tenantId, CancellationToken cancellationToken)
    {
        await EnviarParaAdminAsync(tenantId,
            "Sua assinatura foi suspensa",
            "<p>Olá,</p><p>A assinatura da sua organização na plataforma foi <strong>suspensa</strong> por falta de pagamento. " +
            "Para reativar o acesso, regularize o pagamento na área de <strong>Assinatura</strong> do sistema.</p>",
            cancellationToken);
    }

    private async Task EnviarParaAdminAsync(int tenantId, string assunto, string html, CancellationToken cancellationToken)
    {
        try
        {
            var email = await _context.Usuarios
                .Where(u => u.TenantId == tenantId && u.Ativo && u.TipoUsuario == TipoUsuario.Admin)
                .Select(u => u.EmailLogin)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            await _emailService.SendAsync(new EmailMessage
            {
                To = email,
                Subject = assunto,
                HtmlBody = html
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            // Notificação é best-effort: nunca derruba o ciclo.
            _logger.LogWarning(ex, "Falha ao notificar tenant {TenantId} ('{Assunto}').", tenantId, assunto);
        }
    }
}
