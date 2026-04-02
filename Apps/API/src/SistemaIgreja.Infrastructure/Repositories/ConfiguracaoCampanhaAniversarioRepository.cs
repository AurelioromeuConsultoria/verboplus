using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class ConfiguracaoCampanhaAniversarioRepository : IConfiguracaoCampanhaAniversarioRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public ConfiguracaoCampanhaAniversarioRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<ConfiguracaoCampanhaAniversario> GetAsync()
    {
        var configuracao = await _context.ConfiguracoesCampanhaAniversario.FirstOrDefaultAsync();
        if (configuracao != null)
        {
            return configuracao;
        }

        configuracao = new ConfiguracaoCampanhaAniversario
        {
            Id = 1,
            Ativo = true,
            ImagemUrl = "/assets/birthday/niver-whatsapp-aniversario.png",
            HorarioEnvio = new TimeSpan(9, 0, 0),
            MensagemTemplate = CampanhaAniversarioDefaults.MensagemTemplatePadrao,
            DataAtualizacao = DateTime.Now
        };

        _context.ConfiguracoesCampanhaAniversario.Add(configuracao);
        await _context.SaveChangesAsync();
        return configuracao;
    }

    public async Task<ConfiguracaoCampanhaAniversario> UpdateAsync(ConfiguracaoCampanhaAniversario configuracao)
    {
        var existente = await _context.ConfiguracoesCampanhaAniversario.FirstOrDefaultAsync();
        if (existente == null)
        {
            configuracao.Id = 1;
            configuracao.DataAtualizacao = DateTime.Now;
            _context.ConfiguracoesCampanhaAniversario.Add(configuracao);
            await _context.SaveChangesAsync();
            return configuracao;
        }

        existente.Ativo = configuracao.Ativo;
        existente.ImagemUrl = configuracao.ImagemUrl;
        existente.MensagemTemplate = configuracao.MensagemTemplate;
        existente.HorarioEnvio = configuracao.HorarioEnvio;
        existente.DataAtualizacao = DateTime.Now;

        await _context.SaveChangesAsync();
        return existente;
    }
}
