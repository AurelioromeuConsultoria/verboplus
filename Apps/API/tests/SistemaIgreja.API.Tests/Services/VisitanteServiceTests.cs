using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class VisitanteServiceTests
{
    private readonly Mock<IVisitanteRepository> _repoMock = new();
    private readonly Mock<IMensagemAgendadaService> _msgServiceMock = new();
    private readonly Mock<IPessoaRepository> _pessoaRepoMock = new();
    private readonly Mock<IPessoaPerfilRepository> _pessoaPerfilRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly VisitanteService _service;

    public VisitanteServiceTests()
    {
        _service = new VisitanteService(_repoMock.Object, _msgServiceMock.Object, _pessoaRepoMock.Object, _pessoaPerfilRepoMock.Object, _unitOfWorkMock.Object);
        
        // Setup padrão para UnitOfWork
        _unitOfWorkMock.Setup(u => u.BeginTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.CommitTransactionAsync()).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.RollbackTransactionAsync()).Returns(Task.CompletedTask);
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
        _msgServiceMock.Verify(m => m.AgendarMensagensParaVisitanteAsync(5), Times.Once);
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
