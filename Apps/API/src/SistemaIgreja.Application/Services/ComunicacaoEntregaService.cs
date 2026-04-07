using Microsoft.Extensions.Logging;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IComunicacaoEntregaService
{
    Task<PagedResultDto<ComunicacaoEntregaResumoDto>> GetPagedAsync(ComunicacaoEntregaPagedQueryDto query);
    Task<IReadOnlyList<ComunicacaoEntregaResumoDto>> GetByCampanhaIdAsync(int campanhaId);
    Task<IReadOnlyList<ComunicacaoEntregaResumoDto>> ReservarPendentesAsync(int limit);
    Task MarcarComoEnviadaAsync(int entregaId);
    Task MarcarComoFalhaAsync(int entregaId, string erro);
}

public class ComunicacaoEntregaService : IComunicacaoEntregaService
{
    private readonly IComunicacaoEntregaRepository _repository;
    private readonly ILogger<ComunicacaoEntregaService> _logger;
    private readonly IAuditLogService _auditLogService;

    public ComunicacaoEntregaService(
        IComunicacaoEntregaRepository repository,
        ILogger<ComunicacaoEntregaService> logger,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task<PagedResultDto<ComunicacaoEntregaResumoDto>> GetPagedAsync(ComunicacaoEntregaPagedQueryDto query)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 200);
        var (items, total) = await _repository.GetPagedAsync(query);

        return new PagedResultDto<ComunicacaoEntregaResumoDto>
        {
            Items = items.Select(MapResumo).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IReadOnlyList<ComunicacaoEntregaResumoDto>> GetByCampanhaIdAsync(int campanhaId)
    {
        var items = await _repository.GetByCampanhaIdAsync(campanhaId);
        return items.Select(MapResumo).ToList();
    }

    public async Task<IReadOnlyList<ComunicacaoEntregaResumoDto>> ReservarPendentesAsync(int limit)
    {
        var items = await _repository.ReservarPendentesAsync(limit);
        if (items.Count > 0)
        {
            _logger.LogInformation(
                "{EventName} Quantidade={Quantidade}",
                ComunicacaoObservability.Events.EntregaReservada,
                items.Count);
        }

        return items.Select(MapResumo).ToList();
    }

    public async Task MarcarComoEnviadaAsync(int entregaId)
    {
        var entrega = await _repository.GetByIdAsync(entregaId) ?? throw new ArgumentException("Entrega não encontrada");
        entrega.Status = StatusComunicacaoEntrega.Enviado;
        entrega.EntregueEm = DateTime.UtcNow;
        entrega.ProcessadoEm = DateTime.UtcNow;
        entrega.Tentativas += 1;
        await _repository.UpdateAsync(entrega);

        _logger.LogInformation(
            "{EventName} EntregaId={EntregaId} Canal={Canal}",
            ComunicacaoObservability.Events.EntregaEnviada,
            entrega.Id,
            entrega.Canal);
    }

    public async Task MarcarComoFalhaAsync(int entregaId, string erro)
    {
        var entrega = await _repository.GetByIdAsync(entregaId) ?? throw new ArgumentException("Entrega não encontrada");
        entrega.Status = StatusComunicacaoEntrega.Falhou;
        entrega.Erro = erro;
        entrega.ProcessadoEm = DateTime.UtcNow;
        entrega.Tentativas += 1;
        await _repository.UpdateAsync(entrega);

        _logger.LogWarning(
            "{EventName} EntregaId={EntregaId} Canal={Canal} Erro={Erro}",
            ComunicacaoObservability.Events.EntregaFalhou,
            entrega.Id,
            entrega.Canal,
            erro);

        await _auditLogService.RecordAsync("ComunicacaoEntrega", entrega.Id.ToString(), "Falha", new
        {
            entrega.Canal,
            entrega.DestinoResolvido,
            entrega.Erro
        });
    }

    private static ComunicacaoEntregaResumoDto MapResumo(ComunicacaoEntrega entrega)
    {
        return new ComunicacaoEntregaResumoDto
        {
            Id = entrega.Id,
            Canal = entrega.Canal,
            DestinoResolvido = entrega.DestinoResolvido,
            Status = entrega.Status,
            Tentativas = entrega.Tentativas,
            ProcessadoEm = entrega.ProcessadoEm,
            EntregueEm = entrega.EntregueEm,
            Erro = entrega.Erro,
            MidiaUrl = entrega.MidiaUrl
        };
    }
}
