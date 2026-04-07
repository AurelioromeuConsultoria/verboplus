using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.DTOs.Pessoas;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class PessoaServiceTests
{
    private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
    private readonly Mock<IPessoaPerfilRepository> _perfilRepositoryMock = new();
    private readonly Mock<IVisitanteService> _visitanteServiceMock = new();
    private readonly Mock<IVoluntarioService> _voluntarioServiceMock = new();
    private readonly Mock<IUsuarioService> _usuarioServiceMock = new();
    private readonly Mock<ILogger<PessoaService>> _loggerMock = new();
    private readonly PessoaService _service;

    public PessoaServiceTests()
    {
        _service = new PessoaService(
            _pessoaRepositoryMock.Object,
            _perfilRepositoryMock.Object,
            _visitanteServiceMock.Object,
            _voluntarioServiceMock.Object,
            _usuarioServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEmailAlreadyExists()
    {
        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync("pessoa@app.com"))
            .ReturnsAsync(new Pessoa { Id = 1, Email = "pessoa@app.com" });

        var act = () => _service.CreateAsync(new CriarPessoaDto
        {
            Nome = "Pessoa Duplicada",
            Email = "pessoa@app.com",
            TipoPessoa = TipoPessoa.Adulto
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email já cadastrado");
    }

    [Fact]
    public async Task CreateAsync_CreatesPessoa_WhenEmailIsAvailable()
    {
        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync("nova@app.com"))
            .ReturnsAsync((Pessoa?)null);
        _pessoaRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Pessoa>()))
            .ReturnsAsync((Pessoa pessoa) =>
            {
                pessoa.Id = 15;
                return pessoa;
            });
        _perfilRepositoryMock.Setup(r => r.GetPerfisPorPessoaAsync(15))
            .ReturnsAsync(new List<PessoaPerfil>());

        var result = await _service.CreateAsync(new CriarPessoaDto
        {
            Nome = "Nova Pessoa",
            Email = "nova@app.com",
            TipoPessoa = TipoPessoa.Adulto
        });

        result.Id.Should().Be(15);
        result.Nome.Should().Be("Nova Pessoa");
        result.Email.Should().Be("nova@app.com");
        result.TipoPessoa.Should().Be(TipoPessoa.Adulto);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEmailBelongsToAnotherPessoa()
    {
        var pessoa = new Pessoa
        {
            Id = 15,
            Nome = "Pessoa Original",
            Email = "original@app.com",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true
        };

        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(pessoa);
        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync("duplicado@app.com"))
            .ReturnsAsync(new Pessoa { Id = 99, Email = "duplicado@app.com" });

        var act = () => _service.UpdateAsync(15, new AtualizarPessoaDto
        {
            Nome = "Pessoa Atualizada",
            Email = "duplicado@app.com",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email já cadastrado para outra pessoa");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPessoa_WhenDataIsValid()
    {
        var pessoa = new Pessoa
        {
            Id = 15,
            Nome = "Pessoa Original",
            Email = "original@app.com",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true
        };

        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(pessoa);
        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync("nova@app.com"))
            .ReturnsAsync((Pessoa?)null);
        _pessoaRepositoryMock.Setup(r => r.UpdateAsync(pessoa)).ReturnsAsync(pessoa);
        _perfilRepositoryMock.Setup(r => r.GetPerfisPorPessoaAsync(15))
            .ReturnsAsync(new List<PessoaPerfil>());

        var result = await _service.UpdateAsync(15, new AtualizarPessoaDto
        {
            Nome = "Pessoa Atualizada",
            Email = "nova@app.com",
            Telefone = "1111",
            WhatsApp = "2222",
            TipoPessoa = TipoPessoa.Crianca,
            Ativo = false
        });

        result.Id.Should().Be(15);
        pessoa.Nome.Should().Be("Pessoa Atualizada");
        pessoa.Email.Should().Be("nova@app.com");
        pessoa.TipoPessoa.Should().Be(TipoPessoa.Crianca);
        pessoa.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Get360Async_ReturnsAggregatedData()
    {
        var pessoa = new Pessoa
        {
            Id = 15,
            Nome = "Pessoa 360",
            Email = "pessoa360@app.com",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true
        };

        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(15)).ReturnsAsync(pessoa);
        _perfilRepositoryMock.Setup(r => r.GetPerfisPorPessoaAsync(15))
            .ReturnsAsync(new List<PessoaPerfil>
            {
                new()
                {
                    Id = 1,
                    PessoaId = 15,
                    Perfil = PerfilPessoa.Membro,
                    DataInicio = DateTime.UtcNow
                }
            });
        _visitanteServiceMock.Setup(s => s.GetVisitantesPorPessoaAsync(15))
            .ReturnsAsync(new List<VisitanteDto>
            {
                new() { Id = 1, PessoaId = 15, DataVisita = DateTime.UtcNow }
            });
        _voluntarioServiceMock.Setup(s => s.GetVoluntariosPorPessoaAsync(15))
            .ReturnsAsync(new List<VoluntarioDto>
            {
                new() { Id = 2, PessoaId = 15, Nome = "Pessoa 360" }
            });
        _usuarioServiceMock.Setup(s => s.GetByPessoaIdAsync(15))
            .ReturnsAsync(new UsuarioDto
            {
                Id = 99,
                PessoaId = 15,
                EmailLogin = "pessoa360@app.com",
                TipoUsuarioDescricao = "Administrador",
                Ativo = true
            });

        var result = await _service.Get360Async(15);

        result.Should().NotBeNull();
        result!.Pessoa.Id.Should().Be(15);
        result.Visitantes.Should().HaveCount(1);
        result.Voluntarios.Should().HaveCount(1);
        result.Usuario.Should().NotBeNull();
        result.Usuario!.Id.Should().Be(99);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsPagedResult()
    {
        var pessoa = new Pessoa
        {
            Id = 15,
            Nome = "Pessoa Paginada",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true,
            Perfis = new List<PessoaPerfil>
            {
                new()
                {
                    Id = 1,
                    PessoaId = 15,
                    Perfil = PerfilPessoa.Membro,
                    DataInicio = DateTime.UtcNow
                }
            }
        };

        _pessoaRepositoryMock.Setup(r => r.GetPagedAsync(It.IsAny<PessoaPagedQuery>()))
            .ReturnsAsync((new List<Pessoa> { pessoa }, 1));

        var result = await _service.GetPagedAsync(new PessoaPagedQueryDto
        {
            Page = 1,
            PageSize = 10,
            Nome = "Pessoa"
        });

        result.Total.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].Nome.Should().Be("Pessoa Paginada");
        result.Items[0].Perfis.Should().HaveCount(1);
    }
}
