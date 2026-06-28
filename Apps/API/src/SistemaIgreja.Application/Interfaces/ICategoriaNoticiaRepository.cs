using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Interfaces;

public interface ICategoriaNoticiaRepository
{
    Task<IEnumerable<CategoriaNoticia>> GetAllAsync();
    Task<CategoriaNoticia?> GetByIdAsync(int id);
    Task<CategoriaNoticia> CreateAsync(CategoriaNoticia categoriaNoticia);
    Task<CategoriaNoticia> UpdateAsync(CategoriaNoticia categoriaNoticia);
    Task DeleteAsync(int id);
}



