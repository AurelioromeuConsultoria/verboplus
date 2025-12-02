using SistemaIgreja.Application.DTOs;

namespace SistemaIgreja.Application.Interfaces;

public class ArquivoUpload
{
    public string NomeArquivo { get; set; } = string.Empty;
    public byte[] Conteudo { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = string.Empty;
}

public interface IGaleriaFotoService
{
    Task<IEnumerable<GaleriaFotoDto>> GetAllAsync();
    Task<IEnumerable<GaleriaFotoDto>> GetAtivasAsync();
    Task<GaleriaFotoDto?> GetByIdAsync(int id);
    Task<IEnumerable<GaleriaFotoDto>> GetByEventoIdAsync(int eventoId);
    Task<IEnumerable<GaleriaFotoDto>> GetByCategoriaMidiaIdAsync(int categoriaMidiaId);
    Task<GaleriaFotoDto> CreateAsync(CriarGaleriaFotoDto dto);
    Task<GaleriaFotoDto> UpdateAsync(int id, AtualizarGaleriaFotoDto dto);
    Task<bool> DeleteAsync(int id);
    Task<bool> UploadFotosAsync(int galeriaId, List<ArquivoUpload> arquivos, string webRootPath);
    Task<bool> DefinirImagemDestaqueAsync(int galeriaId, string nomeArquivo);
    Task<List<FotoDto>> ListarFotosAsync(int galeriaId, string webRootPath);
}

