using SistemaIgreja.Application.DTOs.Search;

namespace SistemaIgreja.Application.Services;

public interface ISearchService
{
    Task<IReadOnlyList<GlobalSearchItemDto>> SearchAsync(string query, int limit);
}

