using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public class GaleriaFotoService : IGaleriaFotoService
{
    private readonly IGaleriaFotoRepository _repository;

    public GaleriaFotoService(IGaleriaFotoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<GaleriaFotoDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GaleriaFotoDto>> GetAtivasAsync()
    {
        var entities = await _repository.GetAtivasAsync();
        return entities.Select(MapToDto);
    }

    public async Task<GaleriaFotoDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity != null ? MapToDto(entity) : null;
    }

    public async Task<IEnumerable<GaleriaFotoDto>> GetByEventoIdAsync(int eventoId)
    {
        var entities = await _repository.GetByEventoIdAsync(eventoId);
        return entities.Select(MapToDto);
    }

    public async Task<IEnumerable<GaleriaFotoDto>> GetByCategoriaMidiaIdAsync(int categoriaMidiaId)
    {
        var entities = await _repository.GetByCategoriaMidiaIdAsync(categoriaMidiaId);
        return entities.Select(MapToDto);
    }

    public async Task<GaleriaFotoDto> CreateAsync(CriarGaleriaFotoDto dto)
    {
        // Criar diretório base para a galeria
        var basePath = Path.Combine("uploads", "fotos");
        var galeriaPath = Path.Combine(basePath, Guid.NewGuid().ToString());

        var entity = new GaleriaFoto
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Data = dto.Data,
            CaminhoDiretorio = galeriaPath,
            EventoId = dto.EventoId,
            CategoriaMidiaId = dto.CategoriaMidiaId,
            Ativo = dto.Ativo,
            QuantidadeFotos = 0,
            DataCriacao = DateTime.Now
        };

        var created = await _repository.CreateAsync(entity);

        // O diretório será criado no momento do upload

        return MapToDto(created);
    }

    public async Task<GaleriaFotoDto> UpdateAsync(int id, AtualizarGaleriaFotoDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new ArgumentException("Galeria não encontrada");

        entity.Nome = dto.Nome;
        entity.Descricao = dto.Descricao;
        entity.Data = dto.Data;
        entity.EventoId = dto.EventoId;
        entity.CategoriaMidiaId = dto.CategoriaMidiaId;
        entity.Ativo = dto.Ativo;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        // O diretório será deletado no controller que tem acesso ao WebRootPath
        return await _repository.DeleteAsync(id);
    }

    public async Task<bool> UploadFotosAsync(int galeriaId, List<ArquivoUpload> arquivos, string webRootPath)
    {
        var galeria = await _repository.GetByIdAsync(galeriaId);
        if (galeria == null) return false;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var maxFileSize = 10 * 1024 * 1024; // 10MB

        var basePath = Path.Combine(webRootPath, galeria.CaminhoDiretorio);
        var originalPath = Path.Combine(basePath, "original");
        var thumbnailPath = Path.Combine(basePath, "thumbnail");
        
        Directory.CreateDirectory(originalPath);
        Directory.CreateDirectory(thumbnailPath);

        var uploadedCount = 0;
        var primeiraFoto = true;
        const int thumbnailSize = 400; // Tamanho máximo do thumbnail (largura ou altura)

        foreach (var arquivo in arquivos)
        {
            if (arquivo.Conteudo.Length == 0 || arquivo.Conteudo.Length > maxFileSize) continue;

            var extension = Path.GetExtension(arquivo.NomeArquivo).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension)) continue;

            var fileName = $"{Guid.NewGuid()}{extension}";
            var originalFilePath = Path.Combine(originalPath, fileName);
            var thumbnailFilePath = Path.Combine(thumbnailPath, fileName);

            // Salvar foto original
            await File.WriteAllBytesAsync(originalFilePath, arquivo.Conteudo);

            // Gerar thumbnail
            try
            {
                using var image = Image.Load(arquivo.Conteudo);
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(thumbnailSize, thumbnailSize),
                    Mode = ResizeMode.Max // Mantém proporção, ajusta para caber no tamanho máximo
                }));

                await image.SaveAsync(thumbnailFilePath);
            }
            catch
            {
                // Se falhar ao processar imagem, continua sem thumbnail
                // A foto original já foi salva
            }

            uploadedCount++;

            // Definir primeira foto como destaque se não houver
            // Usar o thumbnail como destaque (mais leve para carregar)
            if (primeiraFoto && string.IsNullOrEmpty(galeria.ImagemDestaque))
            {
                galeria.ImagemDestaque = Path.Combine(galeria.CaminhoDiretorio, "thumbnail", fileName).Replace("\\", "/");
                primeiraFoto = false;
            }
        }

        // Atualizar quantidade de fotos (contar apenas as originais)
        var fotoFiles = Directory.GetFiles(originalPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .Count();

        galeria.QuantidadeFotos = fotoFiles;
        await _repository.UpdateAsync(galeria);

        return uploadedCount > 0;
    }

    public async Task<bool> DefinirImagemDestaqueAsync(int galeriaId, string nomeArquivo)
    {
        var galeria = await _repository.GetByIdAsync(galeriaId);
        if (galeria == null) return false;

        // Usar o thumbnail como destaque (mais leve para carregar)
        var filePath = Path.Combine(galeria.CaminhoDiretorio, "thumbnail", nomeArquivo).Replace("\\", "/");
        galeria.ImagemDestaque = filePath;
        await _repository.UpdateAsync(galeria);

        return true;
    }

    public async Task<List<FotoDto>> ListarFotosAsync(int galeriaId, string webRootPath)
    {
        var galeria = await _repository.GetByIdAsync(galeriaId);
        if (galeria == null) return new List<FotoDto>();

        var fotos = new List<FotoDto>();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        
        var thumbnailPath = Path.Combine(webRootPath, galeria.CaminhoDiretorio, "thumbnail");

        if (!Directory.Exists(thumbnailPath))
        {
            return fotos;
        }

        var arquivos = Directory.GetFiles(thumbnailPath, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .OrderBy(f => Path.GetFileName(f))
            .ToList();

        foreach (var arquivo in arquivos)
        {
            var nomeArquivo = Path.GetFileName(arquivo);
            var isDestaque = galeria.ImagemDestaque != null && 
                           galeria.ImagemDestaque.Contains(nomeArquivo);

            fotos.Add(new FotoDto
            {
                NomeArquivo = nomeArquivo,
                Destaque = isDestaque
            });
        }

        return await Task.FromResult(fotos);
    }

    private static GaleriaFotoDto MapToDto(GaleriaFoto g)
    {
        return new GaleriaFotoDto
        {
            Id = g.Id,
            Nome = g.Nome,
            Descricao = g.Descricao,
            Data = g.Data,
            CaminhoDiretorio = g.CaminhoDiretorio,
            ImagemDestaque = g.ImagemDestaque,
            QuantidadeFotos = g.QuantidadeFotos,
            Ativo = g.Ativo,
            EventoId = g.EventoId,
            EventoTitulo = g.Evento?.Titulo,
            CategoriaMidiaId = g.CategoriaMidiaId,
            CategoriaMidiaNome = g.CategoriaMidia?.Nome,
            DataCriacao = g.DataCriacao
        };
    }
}

