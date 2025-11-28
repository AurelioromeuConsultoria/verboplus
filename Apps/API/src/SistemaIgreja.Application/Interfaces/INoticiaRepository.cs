using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface INoticiaRepository
{
    Task<IEnumerable<Noticia>> GetAllAsync();
    Task<Noticia?> GetByIdAsync(int id);
    Task<IEnumerable<Noticia>> GetByCategoriaAsync(int categoriaId);
    Task<Noticia> CreateAsync(Noticia noticia);
    Task<Noticia> UpdateAsync(Noticia noticia);
    Task DeleteAsync(int id);
}



