using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface IKidsDeviceTokenRepository
{
    Task UpsertAsync(int pessoaId, string fcmToken, string platform);
    Task<IEnumerable<string>> GetTokensByPessoaIdsAsync(IEnumerable<int> pessoaIds);
}
