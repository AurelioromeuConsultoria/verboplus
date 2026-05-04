using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class VisitanteServiceTests
{
    private readonly Mock<IVisitanteRepository> _repoMock = new();
    private readonly Mock<IComunicacaoAutomacaoService> _comunicacaoAutomacaoServiceMock = new();
    private readonly Mock<IPessoaRepository> _pessoaRepoMock = new();
    private readonly Mock<IPessoaPerfilRepository> _pessoaPerfilRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ILogger<VisitanteService>> _loggerMock = new();
    private readonly VisitanteService _service;

    public VisitanteServiceTests()
    {
        _service = new VisitanteService(_repoMock.Object, _comunicacaoAutomacaoServiceMock.Object, _pessoaRepoMock.Object, _pessoaPerfilRepoMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
        
        // Setup UnitOfWork: ExecuteInTransactionAsync deve executar o delegate passado
        _unitOfWorkMock.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async (action) => await action());
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
    }

    [Fact]
    public async Task CreateAsync_CreatesVisitor_AndSchedulesMessages()
    {
        var dto = new CriarVisitanteDto { Nome = "Joao", Telefone = "123", DataVisita = DateTime.UtcNow };
        
        // Mock: pessoa não existe, então será criada
        _pessoaRepoMock.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Pessoa?)null);
        _pessoaRepoMock.Setup(r => r.GetByWhatsAppAsync(It.IsAny<string>())).ReturnsAsync((Pessoa?)null);
        _pessoaRepoMock.Setup(r => r.GetByTelefoneAsync(It.IsAny<string>())).ReturnsAsync((Pessoa?)null);
        
        var pessoa = new Pessoa { Id = 1, Nome = dto.Nome, Telefone = dto.Telefone, TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow };
        _pessoaRepoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Pessoa>())).ReturnsAsync(pessoa);
        
        _pessoaPerfilRepoMock.Setup(r => r.GetPerfilAtivoAsync(1, PerfilPessoa.Visitante)).ReturnsAsync((PessoaPerfil?)null);
        _pessoaPerfilRepoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<PessoaPerfil>())).ReturnsAsync((PessoaPerfil p) => p);
        _pessoaPerfilRepoMock.Setup(r => r.GetPerfisPorPessoaAsync(1)).ReturnsAsync(new List<PessoaPerfil> 
        { 
            new() { Id = 1, PessoaId = 1, Perfil = PerfilPessoa.Visitante, DataInicio = DateTime.UtcNow } 
        });
        
        var visitante = new Visitante { Id = 5, PessoaId = 1, Pessoa = pessoa, DataVisita = dto.DataVisita, DataCadastro = DateTime.UtcNow };
        _repoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Visitante>())).ReturnsAsync(visitante);
        _repoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(visitante);

        var result = await _service.CreateAsync(dto);

        result.Id.Should().Be(5);
        _comunicacaoAutomacaoServiceMock.Verify(m => m.ExecutarNovoVisitanteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateVisitanteAsync_NormalizesDateTimes_BeforeSaving()
    {
        var request = new CreateVisitanteRequest
        {
            Nome = "Ronei",
            WhatsApp = "11949633429",
            DataNascimento = new DateTime(1985, 5, 3, 3, 0, 0, DateTimeKind.Utc),
            DataVisita = new DateTime(2026, 5, 3, 3, 0, 0, DateTimeKind.Utc)
        };

        _pessoaRepoMock.Setup(r => r.GetByWhatsAppAsync("11949633429")).ReturnsAsync((Pessoa?)null);
        _pessoaRepoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Pessoa>()))
            .ReturnsAsync((Pessoa p) =>
            {
                p.Id = 11;
                return p;
            });

        _pessoaPerfilRepoMock.Setup(r => r.GetPerfilAtivoAsync(11, PerfilPessoa.Visitante)).ReturnsAsync((PessoaPerfil?)null);
        _pessoaPerfilRepoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<PessoaPerfil>())).ReturnsAsync((PessoaPerfil p) => p);
        _pessoaPerfilRepoMock.Setup(r => r.GetPerfisPorPessoaAsync(11)).ReturnsAsync(new List<PessoaPerfil>
        {
            new() { Id = 1, PessoaId = 11, Perfil = PerfilPessoa.Visitante, DataInicio = DateTime.UtcNow }
        });

        _repoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Visitante>()))
            .ReturnsAsync((Visitante v) =>
            {
                v.Id = 15;
                v.Pessoa = new Pessoa { Id = 11, Nome = request.Nome, WhatsApp = request.WhatsApp };
                return v;
            });
        _repoMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(new Visitante
        {
            Id = 15,
            PessoaId = 11,
            Pessoa = new Pessoa { Id = 11, Nome = request.Nome, WhatsApp = request.WhatsApp },
            DataVisita = DateTime.SpecifyKind(request.DataVisita.Value, DateTimeKind.Unspecified),
            DataCadastro = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        });

        await _service.CreateVisitanteAsync(request);

        _pessoaRepoMock.Verify(r => r.CreateWithoutSaveAsync(It.Is<Pessoa>(p =>
            p.DataNascimento!.Value.Kind == DateTimeKind.Unspecified &&
            p.DataCriacao.Kind == DateTimeKind.Unspecified)));
        _pessoaPerfilRepoMock.Verify(r => r.CreateWithoutSaveAsync(It.Is<PessoaPerfil>(p =>
            p.DataInicio.Kind == DateTimeKind.Unspecified)));
        _repoMock.Verify(r => r.CreateWithoutSaveAsync(It.Is<Visitante>(v =>
            v.DataVisita.Kind == DateTimeKind.Unspecified &&
            v.DataCadastro.Kind == DateTimeKind.Unspecified)));
    }

    [Fact]
    public async Task CreateVisitanteAsync_SavesNewPessoa_BeforeCreatingPerfilAndVisitante()
    {
        var request = new CreateVisitanteRequest
        {
            Nome = "Ronei",
            WhatsApp = "11949633429",
            DataVisita = DateTime.UtcNow
        };
        Pessoa? pessoaCriada = null;

        _pessoaRepoMock.Setup(r => r.GetByWhatsAppAsync("11949633429")).ReturnsAsync((Pessoa?)null);
        _pessoaRepoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Pessoa>()))
            .ReturnsAsync((Pessoa p) =>
            {
                pessoaCriada = p;
                return p;
            });
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(() =>
            {
                if (pessoaCriada is { Id: 0 })
                {
                    pessoaCriada.Id = 22;
                }

                return 1;
            });
        _pessoaPerfilRepoMock.Setup(r => r.GetPerfilAtivoAsync(22, PerfilPessoa.Visitante)).ReturnsAsync((PessoaPerfil?)null);
        _pessoaPerfilRepoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<PessoaPerfil>())).ReturnsAsync((PessoaPerfil p) => p);
        _pessoaPerfilRepoMock.Setup(r => r.GetPerfisPorPessoaAsync(22)).ReturnsAsync(new List<PessoaPerfil>
        {
            new() { Id = 1, PessoaId = 22, Perfil = PerfilPessoa.Visitante, DataInicio = DateTime.UtcNow }
        });
        _repoMock.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Visitante>()))
            .ReturnsAsync((Visitante v) =>
            {
                v.Id = 33;
                v.Pessoa = new Pessoa { Id = 22, Nome = request.Nome, WhatsApp = request.WhatsApp };
                return v;
            });
        _repoMock.Setup(r => r.GetByIdAsync(33)).ReturnsAsync(new Visitante
        {
            Id = 33,
            PessoaId = 22,
            Pessoa = new Pessoa { Id = 22, Nome = request.Nome, WhatsApp = request.WhatsApp },
            DataVisita = DateTime.SpecifyKind(request.DataVisita.Value, DateTimeKind.Unspecified),
            DataCadastro = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
        });

        await _service.CreateVisitanteAsync(request);

        _pessoaPerfilRepoMock.Verify(r => r.CreateWithoutSaveAsync(It.Is<PessoaPerfil>(p => p.PessoaId == 22)));
        _repoMock.Verify(r => r.CreateWithoutSaveAsync(It.Is<Visitante>(v => v.PessoaId == 22)));
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Visitante?)null);
        var dto = new AtualizarVisitanteDto { DataVisita = DateTime.UtcNow, Observacoes = "Teste" };
        await _service.Invoking(s => s.UpdateAsync(1, dto)).Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        _repoMock.Setup(r => r.DeleteAsync(7)).Returns(Task.CompletedTask);
        await _service.DeleteAsync(7);
        _repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
    }
}
