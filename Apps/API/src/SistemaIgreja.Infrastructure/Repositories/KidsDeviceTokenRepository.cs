using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;

namespace SistemaIgreja.Infrastructure.Repositories;

public class KidsDeviceTokenRepository : IKidsDeviceTokenRepository
{
    private readonly SistemaIgrejaDbContext _context;

    public KidsDeviceTokenRepository(SistemaIgrejaDbContext context)
    {
        _context = context;
    }

    public async Task UpsertAsync(int pessoaId, string fcmToken, string platform)
    {
        var normalized = platform?.Trim().ToUpperInvariant() ?? "ANDROID";
        if (normalized != "ANDROID" && normalized != "IOS") normalized = "ANDROID";

        var existing = await _context.Set<KidsDeviceToken>()
            .FirstOrDefaultAsync(t => t.PessoaId == pessoaId && t.FcmToken == fcmToken);

        if (existing != null)
        {
            existing.Platform = normalized;
            existing.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return;
        }

        _context.Set<KidsDeviceToken>().Add(new KidsDeviceToken
        {
            PessoaId = pessoaId,
            FcmToken = fcmToken,
            Platform = normalized,
            UpdatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<string>> GetTokensByPessoaIdsAsync(IEnumerable<int> pessoaIds)
    {
        var ids = pessoaIds.Distinct().ToList();
        if (ids.Count == 0) return Array.Empty<string>();

        return await _context.Set<KidsDeviceToken>()
            .Where(t => ids.Contains(t.PessoaId))
            .Select(t => t.FcmToken)
            .ToListAsync();
    }
}
