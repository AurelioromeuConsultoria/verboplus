using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class MembroCadastroServiceTests
{
    private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
    private readonly Mock<IPessoaPerfilRepository> _pessoaPerfilRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICadastroMembroNotificationService> _notificationServiceMock = new();
    private readonly Mock<ILogger<MembroCadastroService>> _loggerMock = new();
    private readonly MembroCadastroService _service;

    public MembroCadastroServiceTests()
    {
        _service = new MembroCadastroService(
            _pessoaRepositoryMock.Object,
            _pessoaPerfilRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _notificationServiceMock.Object,
            _loggerMock.Object);

        _unitOfWorkMock
            .Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task>>()))
            .Returns<Func<Task>>(async action => await action());

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        _notificationServiceMock
            .Setup(n => n.NotifySuccessAsync(It.IsAny<CadastroMembroNotification>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CadastroMembroNotificationResult
            {
                WhatsApp = new CadastroMembroCanalResultado { Status = "sent", Mensagem = "Mensagem enviada com sucesso." },
                Email = new CadastroMembroCanalResultado { Status = "skipped", Mensagem = "E-mail desabilitado." }
            });
    }

    [Fact]
    public async Task CadastrarAsync_CriaPessoaNova_ComPerfilMembroENotificacoes()
    {
        var nascimento = DateTime.UtcNow.AddYears(-10);
        var dto = new CadastroMembroDto
        {
            Nome = "Ana Souza",
            Email = "ana@email.com",
            WhatsApp = "(11) 98888-7777",
            DataNascimento = nascimento
        };

        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((Pessoa?)null);
        _pessoaRepositoryMock
            .Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Pessoa>()))
            .ReturnsAsync((Pessoa pessoa) =>
            {
                pessoa.Id = 42;
                return pessoa;
            });

        _pessoaPerfilRepositoryMock
            .Setup(r => r.GetPerfilAtivoAsync(42, PerfilPessoa.Membro))
            .ReturnsAsync((PessoaPerfil?)null);

        var result = await _service.CadastrarAsync(dto);

        result.Sucesso.Should().BeTrue();
        result.PessoaId.Should().Be(42);
        result.Mensagem.Should().Be("Cadastro realizado com sucesso!");
        result.WhatsApp?.Status.Should().Be("sent");

        _pessoaRepositoryMock.Verify(r => r.CreateWithoutSaveAsync(It.Is<Pessoa>(p =>
            p.Nome == "Ana Souza" &&
            p.Email == "ana@email.com" &&
            p.WhatsApp == "11988887777" &&
            p.TipoPessoa == TipoPessoa.Crianca)), Times.Once);

        _pessoaPerfilRepositoryMock.Verify(r => r.CreateWithoutSaveAsync(It.Is<PessoaPerfil>(p =>
            p.PessoaId == 42 && p.Perfil == PerfilPessoa.Membro)), Times.Once);

        _notificationServiceMock.Verify(n => n.NotifySuccessAsync(It.Is<CadastroMembroNotification>(m =>
            m.Nome == "Ana Souza" &&
            m.Email == "ana@email.com" &&
            m.WhatsApp == "11988887777"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CadastrarAsync_ComplementaPessoaExistenteSemDuplicar()
    {
        var pessoaExistente = new Pessoa
        {
            Id = 7,
            Nome = "Carlos",
            Email = "carlos@email.com",
            WhatsApp = null,
            DataNascimento = null,
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            DataCriacao = DateTime.UtcNow
        };

        var dto = new CadastroMembroDto
        {
            Nome = "Carlos",
            Email = "carlos@email.com",
            WhatsApp = "11977776666",
            DataNascimento = DateTime.UtcNow.AddYears(-20)
        };

        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync(pessoaExistente);
        _pessoaPerfilRepositoryMock.Setup(r => r.GetPerfilAtivoAsync(7, PerfilPessoa.Membro)).ReturnsAsync((PessoaPerfil?)null);

        var result = await _service.CadastrarAsync(dto);

        result.Sucesso.Should().BeTrue();
        result.PessoaId.Should().Be(7);
        result.Mensagem.Should().Be("Cadastro vinculado a uma pessoa já existente e atualizado com sucesso.");
        result.WhatsApp?.Status.Should().Be("sent");

        _pessoaRepositoryMock.Verify(r => r.UpdateWithoutSaveAsync(It.Is<Pessoa>(p =>
            p.Id == 7 &&
            p.WhatsApp == "11977776666" &&
            p.DataNascimento == dto.DataNascimento)), Times.Once);
        _pessoaRepositoryMock.Verify(r => r.CreateWithoutSaveAsync(It.IsAny<Pessoa>()), Times.Never);
    }

    [Fact]
    public async Task CadastrarAsync_RetornaErro_QuandoDataNascimentoForFutura()
    {
        var result = await _service.CadastrarAsync(new CadastroMembroDto
        {
            Nome = "Teste",
            Email = "teste@email.com",
            WhatsApp = "11999999999",
            DataNascimento = DateTime.UtcNow.AddDays(1)
        });

        result.Sucesso.Should().BeFalse();
        result.Mensagem.Should().Be("Data de nascimento não pode ser futura");
    }

    [Fact]
    public async Task CadastrarAsync_CriaNovaPessoa_QuandoEmailNaoExisteMesmoComWhatsAppJaUsado()
    {
        var dto = new CadastroMembroDto
        {
            Nome = "Conflito",
            Email = "conflito@email.com",
            WhatsApp = "11911112222",
            DataNascimento = DateTime.UtcNow.AddYears(-25)
        };

        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync(dto.Email)).ReturnsAsync((Pessoa?)null);
        _pessoaRepositoryMock
            .Setup(r => r.CreateWithoutSaveAsync(It.IsAny<Pessoa>()))
            .ReturnsAsync((Pessoa pessoa) =>
            {
                pessoa.Id = 99;
                return pessoa;
            });
        _pessoaPerfilRepositoryMock.Setup(r => r.GetPerfilAtivoAsync(99, PerfilPessoa.Membro)).ReturnsAsync((PessoaPerfil?)null);

        var result = await _service.CadastrarAsync(dto);

        result.Sucesso.Should().BeTrue();
        result.PessoaId.Should().Be(99);
        result.Mensagem.Should().Be("Cadastro realizado com sucesso!");
    }
}
