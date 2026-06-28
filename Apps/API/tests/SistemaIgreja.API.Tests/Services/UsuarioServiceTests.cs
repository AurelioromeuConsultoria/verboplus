using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock = new();
    private readonly Mock<IPessoaRepository> _pessoaRepositoryMock = new();
    private readonly Mock<IPerfilAcessoRepository> _perfilAcessoRepositoryMock = new();
    private readonly Mock<ILogger<UsuarioService>> _loggerMock = new();
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _service = new UsuarioService(
            _usuarioRepositoryMock.Object,
            _pessoaRepositoryMock.Object,
            _perfilAcessoRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenEmailLoginAlreadyExists()
    {
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("admin@app.com"))
            .ReturnsAsync(new Usuario { Id = 1, EmailLogin = "admin@app.com" });

        var act = () => _service.CreateAsync(new CriarUsuarioDto
        {
            EmailLogin = "admin@app.com",
            Senha = "Senha123",
            PerfilAcessoId = 1
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email de login já cadastrado");
    }

    [Fact]
    public async Task CreateAsync_CreatesPessoaAndUsuario_WhenPessoaIdIsNotProvided()
    {
        var perfil = CriarPerfil();
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("novo@app.com"))
            .ReturnsAsync((Usuario?)null);
        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync("novo@app.com"))
            .ReturnsAsync((Pessoa?)null);
        _pessoaRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Pessoa>()))
            .ReturnsAsync((Pessoa pessoa) =>
            {
                pessoa.Id = 20;
                return pessoa;
            });
        _usuarioRepositoryMock.Setup(r => r.GetByPessoaIdAsync(20))
            .ReturnsAsync((Usuario?)null);
        _perfilAcessoRepositoryMock.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(perfil);
        _usuarioRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Usuario>()))
            .ReturnsAsync((Usuario usuario) =>
            {
                usuario.Id = 50;
                usuario.Pessoa = new Pessoa
                {
                    Id = 20,
                    Nome = "Novo Usuario",
                    Email = "novo@app.com",
                    TipoPessoa = TipoPessoa.Adulto,
                    Ativo = true
                };
                usuario.PerfilAcesso = perfil;
                return usuario;
            });

        var result = await _service.CreateAsync(new CriarUsuarioDto
        {
            Nome = "Novo Usuario",
            Email = "novo@app.com",
            EmailLogin = "novo@app.com",
            Senha = "Senha123",
            TipoUsuario = TipoUsuario.Admin,
            PerfilAcessoId = 10
        });

        result.Id.Should().Be(50);
        result.PessoaId.Should().Be(20);
        result.EmailLogin.Should().Be("novo@app.com");
        result.PerfilAcessoNome.Should().Be("Administradores");
        _pessoaRepositoryMock.Verify(r => r.CreateAsync(It.Is<Pessoa>(p =>
            p.Nome == "Novo Usuario" &&
            p.Email == "novo@app.com")), Times.Once);
        _usuarioRepositoryMock.Verify(r => r.CreateAsync(It.Is<Usuario>(u =>
            u.PessoaId == 20 &&
            u.EmailLogin == "novo@app.com" &&
            u.PerfilAcessoId == 10)), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenPessoaAlreadyHasUsuario()
    {
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("novo@app.com"))
            .ReturnsAsync((Usuario?)null);
        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(20))
            .ReturnsAsync(new Pessoa { Id = 20, Nome = "Pessoa Base", TipoPessoa = TipoPessoa.Adulto, Ativo = true });
        _usuarioRepositoryMock.Setup(r => r.GetByPessoaIdAsync(20))
            .ReturnsAsync(new Usuario { Id = 2, PessoaId = 20, EmailLogin = "existente@app.com" });

        var act = () => _service.CreateAsync(new CriarUsuarioDto
        {
            PessoaId = 20,
            EmailLogin = "novo@app.com",
            Senha = "Senha123",
            PerfilAcessoId = 10
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Esta pessoa já possui usuário");
    }

    [Fact]
    public async Task UpdateAsync_UpdatesPessoaAndUsuario_WhenDataIsValid()
    {
        var perfil = CriarPerfil();
        var pessoa = new Pessoa
        {
            Id = 20,
            Nome = "Pessoa Antiga",
            Email = "antigo@app.com",
            TipoPessoa = TipoPessoa.Adulto,
            Ativo = true
        };
        var usuario = new Usuario
        {
            Id = 50,
            PessoaId = 20,
            EmailLogin = "antigo@login.com",
            TipoUsuario = TipoUsuario.Portal,
            Ativo = true,
            PerfilAcessoId = 9,
            Pessoa = pessoa,
            PerfilAcesso = perfil
        };

        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(usuario);
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("novo@login.com")).ReturnsAsync((Usuario?)null);
        _pessoaRepositoryMock.Setup(r => r.GetByIdAsync(20)).ReturnsAsync(pessoa);
        _pessoaRepositoryMock.Setup(r => r.GetByEmailAsync("novo@app.com")).ReturnsAsync((Pessoa?)null);
        _pessoaRepositoryMock.Setup(r => r.UpdateAsync(pessoa)).ReturnsAsync(pessoa);
        _perfilAcessoRepositoryMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(perfil);
        _usuarioRepositoryMock.Setup(r => r.UpdateAsync(usuario)).ReturnsAsync(usuario);

        var result = await _service.UpdateAsync(50, new AtualizarUsuarioDto
        {
            Nome = "Pessoa Nova",
            Email = "novo@app.com",
            Telefone = "1111",
            WhatsApp = "2222",
            EmailLogin = "novo@login.com",
            TipoUsuario = TipoUsuario.Admin,
            Ativo = false,
            PerfilAcessoId = 10
        });

        result.Id.Should().Be(50);
        pessoa.Nome.Should().Be("Pessoa Nova");
        pessoa.Email.Should().Be("novo@app.com");
        usuario.EmailLogin.Should().Be("novo@login.com");
        usuario.TipoUsuario.Should().Be(TipoUsuario.Admin);
        usuario.Ativo.Should().BeFalse();
        usuario.PerfilAcessoId.Should().Be(10);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenAnotherUsuarioUsesSameEmailLogin()
    {
        _usuarioRepositoryMock.Setup(r => r.GetByIdAsync(50))
            .ReturnsAsync(new Usuario { Id = 50, PessoaId = 20, EmailLogin = "atual@app.com" });
        _usuarioRepositoryMock.Setup(r => r.GetByEmailAsync("duplicado@app.com"))
            .ReturnsAsync(new Usuario { Id = 99, PessoaId = 21, EmailLogin = "duplicado@app.com" });

        var act = () => _service.UpdateAsync(50, new AtualizarUsuarioDto
        {
            Nome = "Teste",
            EmailLogin = "duplicado@app.com",
            PerfilAcessoId = 10
        });

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Email de login já cadastrado");
    }

    [Fact]
    public async Task DeleteAsync_CallsRepository()
    {
        _usuarioRepositoryMock.Setup(r => r.DeleteAsync(50)).Returns(Task.CompletedTask);

        await _service.DeleteAsync(50);

        _usuarioRepositoryMock.Verify(r => r.DeleteAsync(50), Times.Once);
    }

    private static PerfilAcesso CriarPerfil()
    {
        return new PerfilAcesso
        {
            Id = 10,
            Nome = "Administradores",
            Permissoes = new List<PerfilAcessoPermissao>
            {
                new()
                {
                    Id = 1,
                    Recurso = "Usuarios",
                    PodeVer = true,
                    PodeEditar = true,
                    PodeExcluir = true
                }
            }
        };
    }
}
