using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class EscalaRepository : IEscalaRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public EscalaRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task<Escala?> GetByIdAsync(int id)
    {
        return await _context.Escalas
            .Include(e => e.EventoOcorrencia)
                .ThenInclude(o => o.Evento)
            .Include(e => e.Equipe)
            .Include(e => e.CriadoPorUsuario)
                .ThenInclude(u => u!.Pessoa)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Equipe)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Cargo)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Voluntario)
                    .ThenInclude(v => v.Pessoa)
            .Include(e => e.Itens)
                .ThenInclude(i => i.AprovadoPorUsuario)
                    .ThenInclude(u => u!.Pessoa)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Escala?> GetByEventoOcorrenciaIdAsync(int eventoOcorrenciaId)
    {
        return await _context.Escalas
            .Include(e => e.EventoOcorrencia)
                .ThenInclude(o => o.Evento)
            .Include(e => e.Equipe)
            .Include(e => e.CriadoPorUsuario)
                .ThenInclude(u => u!.Pessoa)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Equipe)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Cargo)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Voluntario)
                    .ThenInclude(v => v.Pessoa)
            .Include(e => e.Itens)
                .ThenInclude(i => i.AprovadoPorUsuario)
                    .ThenInclude(u => u!.Pessoa)
            .FirstOrDefaultAsync(e => e.EventoOcorrenciaId == eventoOcorrenciaId);
    }

    public async Task<Escala?> GetByEventoOcorrenciaAndEquipeAsync(int eventoOcorrenciaId, int equipeId)
    {
        return await _context.Escalas
            .Include(e => e.EventoOcorrencia)
                .ThenInclude(o => o.Evento)
            .Include(e => e.Equipe)
            .Include(e => e.CriadoPorUsuario)
                .ThenInclude(u => u!.Pessoa)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Equipe)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Cargo)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Voluntario)
                    .ThenInclude(v => v.Pessoa)
            .Include(e => e.Itens)
                .ThenInclude(i => i.AprovadoPorUsuario)
                    .ThenInclude(u => u!.Pessoa)
            .FirstOrDefaultAsync(e => e.EventoOcorrenciaId == eventoOcorrenciaId && e.EquipeId == equipeId);
    }

    public async Task<IEnumerable<Escala>> GetAllByEventoOcorrenciaAsync(int eventoOcorrenciaId)
    {
        return await _context.Escalas
            .Include(e => e.EventoOcorrencia)
                .ThenInclude(o => o.Evento)
            .Include(e => e.Equipe)
            .Include(e => e.CriadoPorUsuario)
                .ThenInclude(u => u!.Pessoa)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Equipe)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Cargo)
            .Include(e => e.Itens)
                .ThenInclude(i => i.Voluntario)
                    .ThenInclude(v => v.Pessoa)
            .Where(e => e.EventoOcorrenciaId == eventoOcorrenciaId)
            .OrderBy(e => e.EquipeId)
            .ToListAsync();
    }

    public async Task<Escala> CreateAsync(Escala escala)
    {
        _context.Escalas.Add(escala);
        await _context.SaveChangesAsync();
        return escala;
    }

    public async Task<Escala> UpdateAsync(Escala escala)
    {
        _context.Escalas.Update(escala);
        await _context.SaveChangesAsync();
        return escala;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Escalas.FindAsync(id);
        if (entity == null) return;

        _context.Escalas.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<EscalaItem?> GetItemByIdAsync(int escalaItemId)
    {
        return await _context.EscalasItens
            .Include(i => i.Equipe)
            .Include(i => i.Cargo)
            .Include(i => i.Voluntario)
                .ThenInclude(v => v.Pessoa)
            .Include(i => i.AprovadoPorUsuario)
                .ThenInclude(u => u!.Pessoa)
            .FirstOrDefaultAsync(i => i.Id == escalaItemId);
    }

    public async Task<EscalaItem> AddItemAsync(EscalaItem item)
    {
        _context.EscalasItens.Add(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<EscalaItem> UpdateItemAsync(EscalaItem item)
    {
        _context.EscalasItens.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task DeleteItemAsync(int escalaItemId)
    {
        var entity = await _context.EscalasItens.FindAsync(escalaItemId);
        if (entity == null) return;

        _context.EscalasItens.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<EscalaItem?> GetConflitoPessoaNaEscalaAsync(int escalaId, int voluntarioId, int? ignorarEscalaItemId = null)
    {
        var escala = await _context.Escalas.FindAsync(escalaId);
        if (escala == null) return null;

        var pessoaId = await _context.Voluntarios
            .Where(v => v.Id == voluntarioId)
            .Select(v => v.PessoaId)
            .FirstOrDefaultAsync();

        if (pessoaId == 0) return null;

        // Conflito: mesma pessoa já escalada em qualquer equipe desta ocorrência
        return await _context.EscalasItens
            .Include(i => i.Equipe)
            .Include(i => i.Voluntario)
                .ThenInclude(v => v.Pessoa)
            .Where(i =>
                i.Escala.EventoOcorrenciaId == escala.EventoOcorrenciaId &&
                (!ignorarEscalaItemId.HasValue || i.Id != ignorarEscalaItemId.Value))
            .FirstOrDefaultAsync(i => i.Voluntario.PessoaId == pessoaId);
    }

    public async Task<HashSet<int>> GetPessoaIdsJaEscaladasAsync(int escalaId)
    {
        var escala = await _context.Escalas.FindAsync(escalaId);
        if (escala == null) return new HashSet<int>();

        // Pessoas já escaladas em qualquer equipe desta ocorrência
        var pessoaIds = await _context.EscalasItens
            .Where(i => i.Escala.EventoOcorrenciaId == escala.EventoOcorrenciaId)
            .Select(i => i.Voluntario.PessoaId)
            .Distinct()
            .ToListAsync();

        return pessoaIds.ToHashSet();
    }

    public async Task<Dictionary<int, int>> GetCargaRecentePorVoluntarioAsync(int equipeId, DateTime dataMinima)
    {
        return await _context.EscalasItens
            .Where(i =>
                i.EquipeId == equipeId &&
                i.Escala.EventoOcorrencia.DataHoraInicio >= dataMinima)
            .GroupBy(i => i.VoluntarioId)
            .Select(g => new { VoluntarioId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.VoluntarioId, x => x.Quantidade);
    }

    public async Task<Dictionary<int, int>> GetQuantidadeEscalasNoMesPorVoluntarioAsync(int equipeId, int ano, int mes)
    {
        var inicio = new DateTime(ano, mes, 1, 0, 0, 0);
        var fim = inicio.AddMonths(1).AddTicks(-1);
        return await GetQuantidadeEscalasEmPeriodoPorVoluntarioAsync(equipeId, inicio, fim);
    }

    public async Task<Dictionary<int, int>> GetQuantidadeEscalasEmPeriodoPorVoluntarioAsync(int equipeId, DateTime dataInicio, DateTime dataFim)
    {
        return await _context.EscalasItens
            .Where(i =>
                i.EquipeId == equipeId &&
                i.Escala.EventoOcorrencia.DataHoraInicio >= dataInicio &&
                i.Escala.EventoOcorrencia.DataHoraInicio <= dataFim)
            .GroupBy(i => i.VoluntarioId)
            .Select(g => new { VoluntarioId = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.VoluntarioId, x => x.Quantidade);
    }
}
