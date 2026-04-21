using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;

namespace SistemaIgreja.API.Tests.Services;

public class WorkflowIntegrationTests
{
    [Fact]
    public async Task LoginAsync_WithRealRepository_UpdatesLastAccessAndReturnsTenantData()
    {
        await using var scope = await IntegrationScope.CreateAsync();
        var pessoa = await scope.SeedPessoaAsync("Admin Login", "admin.login@app.com");
        var usuario = await scope.SeedUsuarioAsync(
            pessoa,
            "admin.login@app.com",
            TipoUsuario.Admin,
            senha: "123456");

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateJwtConfiguration(),
            Mock.Of<ILogger<AuthService>>(),
            auditMock.Object);

        var result = await service.LoginAsync(new LoginDto
        {
            Email = usuario.EmailLogin,
            Senha = "123456",
            TenantSlug = Tenant.InitialTenantSlug
        });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Usuario.Id.Should().Be(usuario.Id);
        result.Usuario.TenantSlug.Should().Be(Tenant.InitialTenantSlug);

        var usuarioPersistido = await scope.Context.Usuarios.FirstAsync(x => x.Id == usuario.Id);
        usuarioPersistido.UltimoAcesso.Should().NotBeNull();
        auditMock.Verify(x => x.RecordAsync("Auth", usuario.Id.ToString(), "Login", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRealRepository_RotatesTokenAndKeepsUserActive()
    {
        await using var scope = await IntegrationScope.CreateAsync();
        var pessoa = await scope.SeedPessoaAsync("Admin Refresh", "admin.refresh@app.com");
        var usuario = await scope.SeedUsuarioAsync(
            pessoa,
            "admin.refresh@app.com",
            TipoUsuario.Admin,
            senha: "123456");

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateJwtConfiguration(),
            Mock.Of<ILogger<AuthService>>(),
            auditMock.Object);

        var login = await service.LoginAsync(new LoginDto
        {
            Email = usuario.EmailLogin,
            Senha = "123456",
            TenantSlug = Tenant.InitialTenantSlug
        });

        var refreshed = await service.RefreshTokenAsync(login.RefreshToken);

        refreshed.Token.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBe(login.RefreshToken);
        refreshed.Usuario.Id.Should().Be(usuario.Id);
        auditMock.Verify(x => x.RecordAsync("Auth", usuario.Id.ToString(), "RefreshToken", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AlterarSenhaAsync_WithRealRepository_ChangesHashAndAllowsNewLogin()
    {
        await using var scope = await IntegrationScope.CreateAsync();
        var pessoa = await scope.SeedPessoaAsync("Admin Senha", "admin.senha@app.com");
        var usuario = await scope.SeedUsuarioAsync(
            pessoa,
            "admin.senha@app.com",
            TipoUsuario.Admin,
            senha: "senha-antiga");

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateJwtConfiguration(),
            Mock.Of<ILogger<AuthService>>(),
            auditMock.Object);

        await service.AlterarSenhaAsync(usuario.Id, new AlterarSenhaDto
        {
            SenhaAtual = "senha-antiga",
            NovaSenha = "senha-nova"
        });

        var relido = await new UsuarioRepository(scope.Context, scope.TenantContext).GetByIdAsync(usuario.Id);
        relido.Should().NotBeNull();
        BCrypt.Net.BCrypt.Verify("senha-nova", relido!.SenhaHash).Should().BeTrue();

        var login = await service.LoginAsync(new LoginDto
        {
            Email = usuario.EmailLogin,
            Senha = "senha-nova",
            TenantSlug = Tenant.InitialTenantSlug
        });

        login.Usuario.Id.Should().Be(usuario.Id);
        auditMock.Verify(x => x.RecordAsync("Usuario", usuario.Id.ToString(), "AlterarSenha", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithRealRepository_ThrowsWhenPasswordIsInvalid()
    {
        await using var scope = await IntegrationScope.CreateAsync();
        var pessoa = await scope.SeedPessoaAsync("Admin Invalido", "admin.invalido@app.com");
        var usuario = await scope.SeedUsuarioAsync(
            pessoa,
            "admin.invalido@app.com",
            TipoUsuario.Admin,
            senha: "senha-correta");

        var service = new AuthService(
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateJwtConfiguration(),
            Mock.Of<ILogger<AuthService>>(),
            Mock.Of<IAuditLogService>());

        var act = () => service.LoginAsync(new LoginDto
        {
            Email = usuario.EmailLogin,
            Senha = "senha-errada",
            TenantSlug = Tenant.InitialTenantSlug
        });

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Email ou senha inválidos");
    }

    [Fact]
    public async Task HasPermissionAsync_WithRealRepository_RespectsProfileAndAdminFallback()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var perfil = await scope.SeedPerfilAcessoAsync("Operacao", "Auditoria", podeVer: true, podeEditar: false, podeExcluir: false);
        var pessoaOperador = await scope.SeedPessoaAsync("Operador", "operador@app.com");
        var usuarioOperador = await scope.SeedUsuarioAsync(
            pessoaOperador,
            "operador@app.com",
            TipoUsuario.Portal,
            perfil);

        var pessoaAdmin = await scope.SeedPessoaAsync("Admin Sem Perfil", "admin.sem.perfil@app.com");
        var usuarioAdmin = await scope.SeedUsuarioAsync(
            pessoaAdmin,
            "admin.sem.perfil@app.com",
            TipoUsuario.Admin,
            perfil: null);

        var service = new PermissionService(new UsuarioRepository(scope.Context, scope.TenantContext));

        (await service.HasPermissionAsync(usuarioOperador.Id, "Auditoria", "view")).Should().BeTrue();
        (await service.HasPermissionAsync(usuarioOperador.Id, "Auditoria", "delete")).Should().BeFalse();
        (await service.HasPermissionAsync(usuarioAdmin.Id, "QualquerRecurso", "edit")).Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WithInactiveUser_ReturnsFalse()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var perfil = await scope.SeedPerfilAcessoAsync("Inativo", "Usuarios", podeVer: true, podeEditar: true, podeExcluir: false);
        var pessoa = await scope.SeedPessoaAsync("Usuario Inativo", "inativo@app.com");
        var usuario = await scope.SeedUsuarioAsync(
            pessoa,
            "inativo@app.com",
            TipoUsuario.Portal,
            perfil,
            ativo: false);

        var service = new PermissionService(new UsuarioRepository(scope.Context, scope.TenantContext));

        (await service.HasPermissionAsync(usuario.Id, "Usuarios", "view")).Should().BeFalse();
    }

    [Fact]
    public async Task PublicarAsync_WithRealRepositories_ResetsNonConfirmedItemsAndKeepsConfirmed()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Escala", "lider.escala@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.escala@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Louvor", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Vocal");
        var evento = await scope.SeedEventoAsync("Culto de Domingo");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(7));

        var voluntario1 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Ana Escalada", "ana.escalada@app.com"), equipe, cargo);
        var voluntario2 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Bruno Escalado", "bruno.escalado@app.com"), equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var itemConfirmado = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario1, StatusEscalaItem.Confirmado);
        var itemRecusado = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario2, StatusEscalaItem.Recusado);
        itemRecusado.MotivoRecusa = "Sem disponibilidade";
        itemRecusado.DataRecusa = DateTime.Now.AddHours(-2);
        itemRecusado.RespondidoPorUsuarioId = liderUsuario.Id;
        await scope.Context.SaveChangesAsync();

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var notificacaoMock = CreateNotificationMock();
        var comunicacaoMock = CreateComunicacaoMock();

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            notificacaoMock.Object,
            comunicacaoMock.Object,
            Mock.Of<ILogger<EscalaService>>(),
            auditMock.Object,
            scope.TenantContext);

        var result = await service.PublicarAsync(escala.Id, liderUsuario.Id, false);

        result.Status.Should().Be(StatusEscala.Publicada);
        var escalaPersistida = await new EscalaRepository(scope.Context, scope.TenantContext).GetByIdAsync(escala.Id);
        escalaPersistida!.Status.Should().Be(StatusEscala.Publicada);
        escalaPersistida.DataPublicacao.Should().NotBeNull();
        escalaPersistida.Itens.Single(x => x.Id == itemConfirmado.Id).Status.Should().Be(StatusEscalaItem.Confirmado);

        var itemReprocessado = escalaPersistida.Itens.Single(x => x.Id == itemRecusado.Id);
        itemReprocessado.Status.Should().Be(StatusEscalaItem.Pendente);
        itemReprocessado.MotivoRecusa.Should().BeNull();
        itemReprocessado.DataRecusa.Should().BeNull();
        itemReprocessado.RespondidoPorUsuarioId.Should().BeNull();

        auditMock.Verify(x => x.RecordAsync("Escala", escala.Id.ToString(), "Publicar", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task PublicarAsync_WithRealRepositories_ThrowsWhenUserDoesNotManageTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Oficial", "lider.oficial@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.oficial@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Outro Usuario", "outro.usuario@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "outro.usuario@app.com", TipoUsuario.Portal);
        var equipe = await scope.SeedEquipeAsync("Midia", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Camera");
        var evento = await scope.SeedEventoAsync("Culto Midia");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(2));
        var voluntario = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Operador Midia", "operador.midia@app.com"), equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.PublicarAsync(escala.Id, outroUsuario.Id, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar escalas desta equipe*");
    }

    [Fact]
    public async Task GetAllByEventoOcorrenciaAsync_WithRealRepositories_FiltersByLeaderTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Consulta", "lider.consulta@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.consulta@app.com", TipoUsuario.Admin);
        var outroLiderPessoa = await scope.SeedPessoaAsync("Outro Lider Consulta", "outro.lider.consulta@app.com");
        var outroLiderUsuario = await scope.SeedUsuarioAsync(outroLiderPessoa, "outro.lider.consulta@app.com", TipoUsuario.Admin);

        var equipeLider = await scope.SeedEquipeAsync("Equipe Liderada", liderUsuario.Id);
        var equipeOutra = await scope.SeedEquipeAsync("Equipe Nao Liderada", outroLiderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Servico");
        var evento = await scope.SeedEventoAsync("Evento Consulta Equipe");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(11));

        var voluntario1 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Pessoa Equipe 1", "pessoa.eq1@app.com"), equipeLider, cargo);
        var voluntario2 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Pessoa Equipe 2", "pessoa.eq2@app.com"), equipeOutra, cargo);

        var escalaLider = await scope.SeedEscalaAsync(ocorrencia, equipeLider, liderUsuario);
        var escalaOutra = await scope.SeedEscalaAsync(ocorrencia, equipeOutra, outroLiderUsuario);
        await scope.SeedEscalaItemAsync(escalaLider, equipeLider, cargo, voluntario1, StatusEscalaItem.Pendente);
        await scope.SeedEscalaItemAsync(escalaOutra, equipeOutra, cargo, voluntario2, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var escalas = (await service.GetAllByEventoOcorrenciaAsync(ocorrencia.Id, liderUsuario.Id, false)).ToList();

        escalas.Should().HaveCount(1);
        escalas[0].EquipeId.Should().Be(equipeLider.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WithRealRepositories_ThrowsWhenUserDoesNotManageTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Escala Id", "lider.escala.id@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.escala.id@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Outro Usuario Escala", "outro.usuario.escala@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "outro.usuario.escala@app.com", TipoUsuario.Portal);
        var equipe = await scope.SeedEquipeAsync("Equipe Escala Id", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Escala");
        var evento = await scope.SeedEventoAsync("Evento Escala Id");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(13));
        var voluntario = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Pessoa Escala Id", "pessoa.escala.id@app.com"), equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.GetByIdAsync(escala.Id, outroUsuario.Id, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar escalas desta equipe*");
    }

    [Fact]
    public async Task GetSugestoesAsync_WithRealRepositories_MarksConflictAsUnavailable()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Sugestao", "lider.sugestao@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.sugestao@app.com", TipoUsuario.Admin);
        var equipeA = await scope.SeedEquipeAsync("Equipe A Sugestao", liderUsuario.Id);
        var equipeB = await scope.SeedEquipeAsync("Equipe B Sugestao", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Servico Sugestao");
        var evento = await scope.SeedEventoAsync("Evento Sugestao");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(16));

        var pessoaCompartilhada = await scope.SeedPessoaAsync("Pessoa Compartilhada", "pessoa.compartilhada@app.com");
        var voluntarioEquipeA = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeA, cargo);
        var voluntarioEquipeB = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeB, cargo);

        var escalaA = await scope.SeedEscalaAsync(ocorrencia, equipeA, liderUsuario);
        var escalaB = await scope.SeedEscalaAsync(ocorrencia, equipeB, liderUsuario);
        await scope.SeedEscalaItemAsync(escalaA, equipeA, cargo, voluntarioEquipeA, StatusEscalaItem.Pendente);
        await scope.SeedEscalaItemAsync(escalaB, equipeB, cargo, voluntarioEquipeB, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var sugestoes = (await service.GetSugestoesAsync(escalaB.Id, equipeB.Id, liderUsuario.Id, false)).ToList();

        sugestoes.Should().ContainSingle();
        sugestoes[0].VoluntarioId.Should().Be(voluntarioEquipeB.Id);
        sugestoes[0].Disponivel.Should().BeFalse();
        sugestoes[0].MotivoBloqueio.Should().Be("Já escalado neste evento");
    }

    [Fact]
    public async Task ConfirmarItemAsync_WithRealRepositories_AllowsVolunteerAndUpdatesStatus()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Confirmacao", "lider.confirmacao@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.confirmacao@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Som", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Audio");
        var evento = await scope.SeedEventoAsync("Culto Confirmacao");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(5));

        var pessoaVoluntario = await scope.SeedPessoaAsync("Voluntario Confirmacao", "vol.confirmacao@app.com");
        var usuarioVoluntario = await scope.SeedUsuarioAsync(pessoaVoluntario, "vol.confirmacao@app.com", TipoUsuario.Portal);
        var voluntario = await scope.SeedVoluntarioAsync(pessoaVoluntario, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            auditMock.Object,
            scope.TenantContext);

        var confirmado = await service.ConfirmarItemAsync(escala.Id, item.Id, usuarioVoluntario.Id, false, pessoaVoluntario.Id);

        confirmado.Status.Should().Be(StatusEscalaItem.Confirmado);
        var itemPersistido = await new EscalaRepository(scope.Context, scope.TenantContext).GetItemByIdAsync(item.Id);
        itemPersistido!.Status.Should().Be(StatusEscalaItem.Confirmado);
        itemPersistido.RespondidoPorUsuarioId.Should().Be(usuarioVoluntario.Id);
        itemPersistido.DataConfirmacao.Should().NotBeNull();

        auditMock.Verify(x => x.RecordAsync("EscalaItem", item.Id.ToString(), "Confirmar", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmarItemAsync_WithRealRepositories_ThrowsWhenUserIsNotLeaderAdminOrVolunteer()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Bloqueio Confirmacao", "lider.bloqueio.confirmacao@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.bloqueio.confirmacao@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Usuario Sem Permissao", "usuario.sem.permissao@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "usuario.sem.permissao@app.com", TipoUsuario.Portal);
        var equipe = await scope.SeedEquipeAsync("Equipe Bloqueio Confirmacao", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Confirmacao");
        var evento = await scope.SeedEventoAsync("Evento Bloqueio Confirmacao");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(17));

        var pessoaVoluntario = await scope.SeedPessoaAsync("Voluntario Bloqueio", "vol.bloqueio@app.com");
        var voluntario = await scope.SeedVoluntarioAsync(pessoaVoluntario, equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.ConfirmarItemAsync(escala.Id, item.Id, outroUsuario.Id, false, outroPessoa.Id);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*responder esta escala*");
    }

    [Fact]
    public async Task RecusarItemAsync_WithRealRepositories_StoresReasonAndUpdatesStatus()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Recusa", "lider.recusa@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.recusa@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Intercessao", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Apoio de Oracao");
        var evento = await scope.SeedEventoAsync("Vigilia");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(6));

        var pessoaVoluntario = await scope.SeedPessoaAsync("Voluntario Recusa", "vol.recusa@app.com");
        var usuarioVoluntario = await scope.SeedUsuarioAsync(pessoaVoluntario, "vol.recusa@app.com", TipoUsuario.Portal);
        var voluntario = await scope.SeedVoluntarioAsync(pessoaVoluntario, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            auditMock.Object,
            scope.TenantContext);

        var recusado = await service.RecusarItemAsync(escala.Id, item.Id, "Nao poderei servir", usuarioVoluntario.Id, false, pessoaVoluntario.Id);

        recusado.Status.Should().Be(StatusEscalaItem.Recusado);
        var itemPersistido = await new EscalaRepository(scope.Context, scope.TenantContext).GetItemByIdAsync(item.Id);
        itemPersistido!.Status.Should().Be(StatusEscalaItem.Recusado);
        itemPersistido.MotivoRecusa.Should().Be("Nao poderei servir");
        itemPersistido.RespondidoPorUsuarioId.Should().Be(usuarioVoluntario.Id);
        itemPersistido.DataRecusa.Should().NotBeNull();

        auditMock.Verify(x => x.RecordAsync("EscalaItem", item.Id.ToString(), "Recusar", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task EnviarLembretesPendentesAsync_WithRealRepositories_UpdatesReminderAndCallsAutomation()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var referencia = new DateTime(2026, 4, 21, 10, 0, 0);
        var liderPessoa = await scope.SeedPessoaAsync("Lider Lembrete", "lider.lembrete@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.lembrete@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Lembrete", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Lembrete");
        var evento = await scope.SeedEventoAsync("Evento Lembrete");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, referencia.AddHours(24));

        var pessoaVoluntario = await scope.SeedPessoaAsync("Voluntario Lembrete", "vol.lembrete@app.com");
        await scope.SeedUsuarioAsync(pessoaVoluntario, "vol.lembrete@app.com", TipoUsuario.Portal);
        var voluntario = await scope.SeedVoluntarioAsync(pessoaVoluntario, equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var comunicacaoMock = new Mock<IComunicacaoAutomacaoService>();
        IEnumerable<ComunicacaoLembreteOperacionalRequest>? lembretesCapturados = null;
        comunicacaoMock.Setup(x => x.ExecutarLembretesOperacionaisAsync(
                It.IsAny<IEnumerable<ComunicacaoLembreteOperacionalRequest>>(),
                It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<ComunicacaoLembreteOperacionalRequest>, CancellationToken>((l, _) => lembretesCapturados = l.ToList())
            .ReturnsAsync(1);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            comunicacaoMock.Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var total = await service.EnviarLembretesPendentesAsync(referencia);

        total.Should().Be(1);
        lembretesCapturados.Should().NotBeNull();
        lembretesCapturados.Should().HaveCount(1);
        var itemPersistido = await new EscalaRepository(scope.Context, scope.TenantContext).GetItemByIdAsync(item.Id);
        itemPersistido!.DataLembrete24HorasEnviado.Should().NotBeNull();
        comunicacaoMock.Verify(x => x.ExecutarLembretesOperacionaisAsync(
            It.IsAny<IEnumerable<ComunicacaoLembreteOperacionalRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMinhasEscalasAsync_WithRealRepositories_ReturnsOnlyVolunteerItems()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Minhas", "lider.minhas@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.minhas@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Louvor", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Back Vocal");
        var evento = await scope.SeedEventoAsync("Culto Minhas Escalas");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(9));

        var pessoaAlvo = await scope.SeedPessoaAsync("Voluntario Alvo", "vol.alvo@app.com");
        var voluntarioAlvo = await scope.SeedVoluntarioAsync(pessoaAlvo, equipe, cargo);

        var pessoaOutro = await scope.SeedPessoaAsync("Outro Voluntario", "outro.vol@app.com");
        var voluntarioOutro = await scope.SeedVoluntarioAsync(pessoaOutro, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioAlvo, StatusEscalaItem.Pendente);
        await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioOutro, StatusEscalaItem.Confirmado);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var minhas = (await service.GetMinhasEscalasAsync(pessoaAlvo.Id, true)).ToList();

        minhas.Should().HaveCount(1);
        minhas[0].Itens.Should().HaveCount(1);
        minhas[0].Itens[0].VoluntarioPessoaId.Should().Be(pessoaAlvo.Id);
    }

    [Fact]
    public async Task RegistrarPresencaAsync_WithRealRepositories_UpdatesOperationalStatus()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Presenca", "lider.presenca@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.presenca@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Recepcao", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Organizacao");
        var evento = await scope.SeedEventoAsync("Culto Presenca");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(1));

        var pessoaVoluntario = await scope.SeedPessoaAsync("Voluntario Presenca", "vol.presenca@app.com");
        var voluntario = await scope.SeedVoluntarioAsync(pessoaVoluntario, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Confirmado);

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            auditMock.Object,
            scope.TenantContext);

        var resultado = await service.RegistrarPresencaAsync(escala.Id, item.Id, true, "Serviu normalmente", liderUsuario.Id, false);

        resultado.Status.Should().Be(StatusEscalaItem.Serviu);
        resultado.ObservacaoOperacional.Should().Be("Serviu normalmente");

        var itemPersistido = await new EscalaRepository(scope.Context, scope.TenantContext).GetItemByIdAsync(item.Id);
        itemPersistido!.Status.Should().Be(StatusEscalaItem.Serviu);
        itemPersistido.ObservacaoOperacional.Should().Be("Serviu normalmente");

        auditMock.Verify(x => x.RecordAsync("EscalaItem", item.Id.ToString(), "RegistrarPresenca", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task GetHistoricoVoluntariosAsync_WithRealRepositories_AggregatesOperationalData()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Historico", "lider.historico@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.historico@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Midia", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Projecao");
        var evento = await scope.SeedEventoAsync("Culto Historico");
        var ocorrenciaPassada = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(-3));
        var ocorrenciaFutura = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(4));

        var pessoaVoluntario = await scope.SeedPessoaAsync("Voluntario Historico", "vol.historico@app.com");
        var voluntario = await scope.SeedVoluntarioAsync(pessoaVoluntario, equipe, cargo);

        var escalaPassada = await scope.SeedEscalaAsync(ocorrenciaPassada, equipe, liderUsuario);
        var escalaFutura = await scope.SeedEscalaAsync(ocorrenciaFutura, equipe, liderUsuario);
        await scope.SeedEscalaItemAsync(escalaPassada, equipe, cargo, voluntario, StatusEscalaItem.Serviu);
        await scope.SeedEscalaItemAsync(escalaFutura, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var historico = (await service.GetHistoricoVoluntariosAsync(liderUsuario.Id, false, equipe.Id)).ToList();

        historico.Should().HaveCount(1);
        historico[0].PessoaId.Should().Be(pessoaVoluntario.Id);
        historico[0].Presencas.Should().Be(1);
        historico[0].Pendentes.Should().Be(1);
        historico[0].TotalEscalas.Should().Be(2);
        historico[0].Equipes.Should().Contain("Midia");
    }

    [Fact]
    public async Task AprovarAsync_WithRealRepositories_SubstitutesOriginalItemAndCreatesReplacement()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Troca", "lider.troca@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.troca@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Recepcao", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Apoio");
        var evento = await scope.SeedEventoAsync("Culto da Noite");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(3));

        var pessoaSolicitante = await scope.SeedPessoaAsync("Voluntario Solicitante", "solicitante@app.com");
        var usuarioSolicitante = await scope.SeedUsuarioAsync(pessoaSolicitante, "solicitante@app.com", TipoUsuario.Portal);
        var voluntarioSolicitante = await scope.SeedVoluntarioAsync(pessoaSolicitante, equipe, cargo);

        var pessoaSubstituto = await scope.SeedPessoaAsync("Voluntario Substituto", "substituto@app.com");
        await scope.SeedUsuarioAsync(pessoaSubstituto, "substituto@app.com", TipoUsuario.Portal);
        var voluntarioSubstituto = await scope.SeedVoluntarioAsync(pessoaSubstituto, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var itemOriginal = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioSolicitante, StatusEscalaItem.Pendente);

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var notificacaoMock = CreateNotificationMock();
        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            notificacaoMock.Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            auditMock.Object,
            scope.TenantContext);

        var solicitacao = await service.CreateAsync(
            escala.Id,
            itemOriginal.Id,
            new CriarSolicitacaoTrocaEscalaDto { Motivo = "Compromisso familiar" },
            usuarioSolicitante.Id,
            false,
            pessoaSolicitante.Id);

        var aprovada = await service.AprovarAsync(
            solicitacao.Id,
            new AprovarSolicitacaoTrocaEscalaDto { VoluntarioSubstitutoId = voluntarioSubstituto.Id, ObservacaoResposta = "Troca aprovada" },
            liderUsuario.Id,
            false);

        aprovada.Status.Should().Be(StatusSolicitacaoTrocaEscala.Aprovada);
        aprovada.VoluntarioSubstitutoId.Should().Be(voluntarioSubstituto.Id);

        var escalaPersistida = await new EscalaRepository(scope.Context, scope.TenantContext).GetByIdAsync(escala.Id);
        escalaPersistida!.Itens.Should().HaveCount(2);
        escalaPersistida.Itens.Single(x => x.Id == itemOriginal.Id).Status.Should().Be(StatusEscalaItem.Substituido);

        var novoItem = escalaPersistida.Itens.Single(x => x.Id != itemOriginal.Id);
        novoItem.VoluntarioId.Should().Be(voluntarioSubstituto.Id);
        novoItem.Status.Should().Be(StatusEscalaItem.Pendente);
        novoItem.ObservacaoOperacional.Should().Contain("solicita");

        auditMock.Verify(x => x.RecordAsync("SolicitacaoTrocaEscala", solicitacao.Id.ToString(), "Aprovar", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task AprovarAsync_WithRealRepositories_ThrowsWhenUserDoesNotManageTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Recepcao", "lider.recepcao@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.recepcao@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Usuario Sem Gestao", "sem.gestao@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "sem.gestao@app.com", TipoUsuario.Portal);
        var equipe = await scope.SeedEquipeAsync("Boas Vindas", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Recepcao");
        var evento = await scope.SeedEventoAsync("Culto Especial");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(4));

        var pessoaSolicitante = await scope.SeedPessoaAsync("Pessoa Solicitante", "pessoa.solicitante@app.com");
        var usuarioSolicitante = await scope.SeedUsuarioAsync(pessoaSolicitante, "pessoa.solicitante@app.com", TipoUsuario.Portal);
        var voluntarioSolicitante = await scope.SeedVoluntarioAsync(pessoaSolicitante, equipe, cargo);

        var pessoaSubstituto = await scope.SeedPessoaAsync("Pessoa Substituta", "pessoa.substituta@app.com");
        var voluntarioSubstituto = await scope.SeedVoluntarioAsync(pessoaSubstituto, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var itemOriginal = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioSolicitante, StatusEscalaItem.Pendente);

        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var solicitacao = await service.CreateAsync(
            escala.Id,
            itemOriginal.Id,
            new CriarSolicitacaoTrocaEscalaDto { Motivo = "Viagem" },
            usuarioSolicitante.Id,
            false,
            pessoaSolicitante.Id);

        var act = () => service.AprovarAsync(
            solicitacao.Id,
            new AprovarSolicitacaoTrocaEscalaDto { VoluntarioSubstitutoId = voluntarioSubstituto.Id },
            outroUsuario.Id,
            false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar solicitações desta equipe*");
    }

    [Fact]
    public async Task GetGerenciaveisAsync_WithRealRepositories_ReturnsOnlyLeaderTeamRequests()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Gerenciavel", "lider.gerenciavel@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.gerenciavel@app.com", TipoUsuario.Admin);
        var outraPessoa = await scope.SeedPessoaAsync("Outro Lider", "outro.lider@app.com");
        var outroLider = await scope.SeedUsuarioAsync(outraPessoa, "outro.lider@app.com", TipoUsuario.Admin);

        var equipeLider = await scope.SeedEquipeAsync("Equipe Liderada", liderUsuario.Id);
        var equipeOutra = await scope.SeedEquipeAsync("Equipe Outra", outroLider.Id);
        var cargo = await scope.SeedCargoAsync("Servico");
        var evento = await scope.SeedEventoAsync("Evento Gerenciavel");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(10));

        var pessoaSolicitante1 = await scope.SeedPessoaAsync("Solicitante 1", "sol1@app.com");
        var usuarioSolicitante1 = await scope.SeedUsuarioAsync(pessoaSolicitante1, "sol1@app.com", TipoUsuario.Portal);
        var voluntario1 = await scope.SeedVoluntarioAsync(pessoaSolicitante1, equipeLider, cargo);

        var pessoaSolicitante2 = await scope.SeedPessoaAsync("Solicitante 2", "sol2@app.com");
        var usuarioSolicitante2 = await scope.SeedUsuarioAsync(pessoaSolicitante2, "sol2@app.com", TipoUsuario.Portal);
        var voluntario2 = await scope.SeedVoluntarioAsync(pessoaSolicitante2, equipeOutra, cargo);

        var escala1 = await scope.SeedEscalaAsync(ocorrencia, equipeLider, liderUsuario);
        var escala2 = await scope.SeedEscalaAsync(ocorrencia, equipeOutra, outroLider);
        var item1 = await scope.SeedEscalaItemAsync(escala1, equipeLider, cargo, voluntario1, StatusEscalaItem.Pendente);
        var item2 = await scope.SeedEscalaItemAsync(escala2, equipeOutra, cargo, voluntario2, StatusEscalaItem.Pendente);

        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        await service.CreateAsync(escala1.Id, item1.Id, new CriarSolicitacaoTrocaEscalaDto { Motivo = "Motivo 1" }, usuarioSolicitante1.Id, false, pessoaSolicitante1.Id);
        await service.CreateAsync(escala2.Id, item2.Id, new CriarSolicitacaoTrocaEscalaDto { Motivo = "Motivo 2" }, usuarioSolicitante2.Id, false, pessoaSolicitante2.Id);

        var gerenciaveis = (await service.GetGerenciaveisAsync(liderUsuario.Id, false, null, StatusSolicitacaoTrocaEscala.Pendente)).ToList();

        gerenciaveis.Should().HaveCount(1);
        gerenciaveis[0].EquipeId.Should().Be(equipeLider.Id);
        gerenciaveis[0].VoluntarioSolicitanteNome.Should().Be("Solicitante 1");
    }

    [Fact]
    public async Task GetByEscalaAsync_WithRealRepositories_ThrowsWhenLeaderDoesNotManageTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Escala Troca", "lider.escala.troca@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.escala.troca@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Outro Lider Escala Troca", "outro.lider.escala.troca@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "outro.lider.escala.troca@app.com", TipoUsuario.Portal);
        var equipe = await scope.SeedEquipeAsync("Equipe Escala Troca", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Troca");
        var evento = await scope.SeedEventoAsync("Evento Escala Troca");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(18));

        var pessoaSolicitante = await scope.SeedPessoaAsync("Solicitante Escala Troca", "solicitante.escala.troca@app.com");
        var usuarioSolicitante = await scope.SeedUsuarioAsync(pessoaSolicitante, "solicitante.escala.troca@app.com", TipoUsuario.Portal);
        var voluntario = await scope.SeedVoluntarioAsync(pessoaSolicitante, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        await service.CreateAsync(escala.Id, item.Id, new CriarSolicitacaoTrocaEscalaDto { Motivo = "Pedido" }, usuarioSolicitante.Id, false, pessoaSolicitante.Id);

        var act = () => service.GetByEscalaAsync(escala.Id, outroUsuario.Id, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar solicitações desta equipe*");
    }

    [Fact]
    public async Task CreateAsync_WithRealRepositories_ThrowsWhenPendingRequestAlreadyExists()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Duplicada", "lider.duplicada@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.duplicada@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Duplicada", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Apoio");
        var evento = await scope.SeedEventoAsync("Evento Duplicada");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(14));

        var pessoaSolicitante = await scope.SeedPessoaAsync("Solicitante Duplicado", "solicitante.duplicado@app.com");
        var usuarioSolicitante = await scope.SeedUsuarioAsync(pessoaSolicitante, "solicitante.duplicado@app.com", TipoUsuario.Portal);
        var voluntarioSolicitante = await scope.SeedVoluntarioAsync(pessoaSolicitante, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioSolicitante, StatusEscalaItem.Pendente);

        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        await service.CreateAsync(escala.Id, item.Id, new CriarSolicitacaoTrocaEscalaDto { Motivo = "Primeira" }, usuarioSolicitante.Id, false, pessoaSolicitante.Id);

        var act = () => service.CreateAsync(escala.Id, item.Id, new CriarSolicitacaoTrocaEscalaDto { Motivo = "Segunda" }, usuarioSolicitante.Id, false, pessoaSolicitante.Id);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*solicitação de troca pendente*");
    }

    [Fact]
    public async Task AprovarAsync_WithRealRepositories_ThrowsWhenSubstituteAlreadyHasConflict()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Conflito", "lider.conflito@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.conflito@app.com", TipoUsuario.Admin);
        var equipeA = await scope.SeedEquipeAsync("Equipe A", liderUsuario.Id);
        var equipeB = await scope.SeedEquipeAsync("Equipe B", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Operador");
        var evento = await scope.SeedEventoAsync("Evento Conflito");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(15));

        var pessoaSolicitante = await scope.SeedPessoaAsync("Solicitante Conflito", "solicitante.conflito@app.com");
        var usuarioSolicitante = await scope.SeedUsuarioAsync(pessoaSolicitante, "solicitante.conflito@app.com", TipoUsuario.Portal);
        var voluntarioSolicitante = await scope.SeedVoluntarioAsync(pessoaSolicitante, equipeA, cargo);

        var pessoaSubstituto = await scope.SeedPessoaAsync("Substituto Conflito", "substituto.conflito@app.com");
        var voluntarioSubstitutoEquipeA = await scope.SeedVoluntarioAsync(pessoaSubstituto, equipeA, cargo);
        var voluntarioSubstitutoEquipeB = await scope.SeedVoluntarioAsync(pessoaSubstituto, equipeB, cargo);

        var escalaA = await scope.SeedEscalaAsync(ocorrencia, equipeA, liderUsuario);
        var escalaB = await scope.SeedEscalaAsync(ocorrencia, equipeB, liderUsuario);
        var itemSolicitado = await scope.SeedEscalaItemAsync(escalaA, equipeA, cargo, voluntarioSolicitante, StatusEscalaItem.Pendente);
        await scope.SeedEscalaItemAsync(escalaB, equipeB, cargo, voluntarioSubstitutoEquipeB, StatusEscalaItem.Pendente);

        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var solicitacao = await service.CreateAsync(
            escalaA.Id,
            itemSolicitado.Id,
            new CriarSolicitacaoTrocaEscalaDto { Motivo = "Conflito" },
            usuarioSolicitante.Id,
            false,
            pessoaSolicitante.Id);

        var act = () => service.AprovarAsync(
            solicitacao.Id,
            new AprovarSolicitacaoTrocaEscalaDto { VoluntarioSubstitutoId = voluntarioSubstitutoEquipeA.Id },
            liderUsuario.Id,
            false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*já está escalado neste evento*");
    }

    [Fact]
    public async Task GetMinhasSolicitacoesAsync_WithRealRepositories_ReturnsOnlyPersonRequests()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Minhas Trocas", "lider.minhas.trocas@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.minhas.trocas@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Troca", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Apoio");
        var evento = await scope.SeedEventoAsync("Evento Minhas Trocas");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(12));

        var pessoaAlvo = await scope.SeedPessoaAsync("Pessoa Alvo Troca", "alvo.troca@app.com");
        var usuarioAlvo = await scope.SeedUsuarioAsync(pessoaAlvo, "alvo.troca@app.com", TipoUsuario.Portal);
        var voluntarioAlvo = await scope.SeedVoluntarioAsync(pessoaAlvo, equipe, cargo);

        var pessoaOutra = await scope.SeedPessoaAsync("Pessoa Outra Troca", "outra.troca@app.com");
        var usuarioOutra = await scope.SeedUsuarioAsync(pessoaOutra, "outra.troca@app.com", TipoUsuario.Portal);
        var voluntarioOutro = await scope.SeedVoluntarioAsync(pessoaOutra, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var itemAlvo = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioAlvo, StatusEscalaItem.Pendente);
        var itemOutro = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioOutro, StatusEscalaItem.Pendente);

        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        await service.CreateAsync(escala.Id, itemAlvo.Id, new CriarSolicitacaoTrocaEscalaDto { Motivo = "Minha troca" }, usuarioAlvo.Id, false, pessoaAlvo.Id);
        await service.CreateAsync(escala.Id, itemOutro.Id, new CriarSolicitacaoTrocaEscalaDto { Motivo = "Outra troca" }, usuarioOutra.Id, false, pessoaOutra.Id);

        var minhas = (await service.GetMinhasAsync(pessoaAlvo.Id)).ToList();

        minhas.Should().HaveCount(1);
        minhas[0].VoluntarioSolicitanteNome.Should().Be("Pessoa Alvo Troca");
        minhas[0].Motivo.Should().Be("Minha troca");
    }
    [Fact]
    public async Task RejeitarAsync_WithRealRepositories_MarksRequestAsRejectedWithoutCreatingReplacement()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Rejeicao", "lider.rejeicao@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.rejeicao@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Acolhimento", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Receber");
        var evento = await scope.SeedEventoAsync("Culto Manha");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(8));

        var pessoaSolicitante = await scope.SeedPessoaAsync("Solicitante Rejeicao", "solicitante.rejeicao@app.com");
        var usuarioSolicitante = await scope.SeedUsuarioAsync(pessoaSolicitante, "solicitante.rejeicao@app.com", TipoUsuario.Portal);
        var voluntarioSolicitante = await scope.SeedVoluntarioAsync(pessoaSolicitante, equipe, cargo);

        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var itemOriginal = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntarioSolicitante, StatusEscalaItem.Pendente);

        var auditMock = new Mock<IAuditLogService>();
        auditMock.Setup(x => x.RecordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object?>()))
            .Returns(Task.CompletedTask);

        var service = new SolicitacaoTrocaEscalaService(
            new SolicitacaoTrocaEscalaRepository(scope.Context, scope.TenantContext),
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            Mock.Of<ILogger<SolicitacaoTrocaEscalaService>>(),
            auditMock.Object,
            scope.TenantContext);

        var solicitacao = await service.CreateAsync(
            escala.Id,
            itemOriginal.Id,
            new CriarSolicitacaoTrocaEscalaDto { Motivo = "Imprevisto" },
            usuarioSolicitante.Id,
            false,
            pessoaSolicitante.Id);

        var rejeitada = await service.RejeitarAsync(
            solicitacao.Id,
            new RejeitarSolicitacaoTrocaEscalaDto { ObservacaoResposta = "Nao foi possivel aprovar" },
            liderUsuario.Id,
            false);

        rejeitada.Status.Should().Be(StatusSolicitacaoTrocaEscala.Rejeitada);
        rejeitada.ObservacaoResposta.Should().Be("Nao foi possivel aprovar");

        var escalaPersistida = await new EscalaRepository(scope.Context, scope.TenantContext).GetByIdAsync(escala.Id);
        escalaPersistida!.Itens.Should().HaveCount(1);
        escalaPersistida.Itens.Single().Status.Should().Be(StatusEscalaItem.Pendente);

        auditMock.Verify(x => x.RecordAsync("SolicitacaoTrocaEscala", solicitacao.Id.ToString(), "Rejeitar", It.IsAny<object?>()), Times.Once);
    }

    [Fact]
    public async Task GetByEventoOcorrenciaAsync_WithRealRepositories_ReturnsScaleForManagedLeader()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Evento Ocorrencia", "lider.evento.ocorrencia@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.evento.ocorrencia@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Evento Ocorrencia", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Servico Evento");
        var evento = await scope.SeedEventoAsync("Evento Escala Consulta");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(19));
        var voluntario = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Voluntario Evento", "vol.evento@app.com"), equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var resultado = await service.GetByEventoOcorrenciaAsync(ocorrencia.Id, liderUsuario.Id, false);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(escala.Id);
        resultado.EquipeId.Should().Be(equipe.Id);
        resultado.Itens.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByEventoOcorrenciaAndEquipeAsync_WithRealRepositories_ReturnsOnlyRequestedTeamScale()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Equipe Consulta", "lider.equipe.consulta@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.equipe.consulta@app.com", TipoUsuario.Admin);
        var equipeA = await scope.SeedEquipeAsync("Equipe Consulta A", liderUsuario.Id);
        var equipeB = await scope.SeedEquipeAsync("Equipe Consulta B", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Servico Consulta");
        var evento = await scope.SeedEventoAsync("Evento Consulta Time");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(20));
        var voluntarioA = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Pessoa A", "pessoa.a.consulta@app.com"), equipeA, cargo);
        var voluntarioB = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Pessoa B", "pessoa.b.consulta@app.com"), equipeB, cargo);
        var escalaA = await scope.SeedEscalaAsync(ocorrencia, equipeA, liderUsuario);
        var escalaB = await scope.SeedEscalaAsync(ocorrencia, equipeB, liderUsuario);
        await scope.SeedEscalaItemAsync(escalaA, equipeA, cargo, voluntarioA, StatusEscalaItem.Pendente);
        await scope.SeedEscalaItemAsync(escalaB, equipeB, cargo, voluntarioB, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var resultado = await service.GetByEventoOcorrenciaAndEquipeAsync(ocorrencia.Id, equipeB.Id, liderUsuario.Id, false);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(escalaB.Id);
        resultado.EquipeId.Should().Be(equipeB.Id);
        resultado.Itens.Should().ContainSingle(i => i.VoluntarioPessoaId == voluntarioB.PessoaId);
    }

    [Fact]
    public async Task AddItemAsync_WithRealRepositories_CreatesItemWithNextOrder()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Add Item", "lider.add.item@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.add.item@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Add Item", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Add Item");
        var evento = await scope.SeedEventoAsync("Evento Add Item");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(21));
        var voluntario1 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Primeiro Voluntario Add", "primeiro.add@app.com"), equipe, cargo);
        var voluntario2 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Segundo Voluntario Add", "segundo.add@app.com"), equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario1, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var criado = await service.AddItemAsync(escala.Id, new CriarEscalaItemDto
        {
            EquipeId = equipe.Id,
            CargoId = cargo.Id,
            VoluntarioId = voluntario2.Id
        }, liderUsuario.Id, false);

        criado.VoluntarioId.Should().Be(voluntario2.Id);
        criado.EquipeId.Should().Be(equipe.Id);
        criado.Ordem.Should().Be(2);

        var escalaPersistida = await new EscalaRepository(scope.Context, scope.TenantContext).GetByIdAsync(escala.Id);
        escalaPersistida!.Itens.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateItemAsync_WithRealRepositories_ChangesVolunteerAndOrder()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Update Item", "lider.update.item@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.update.item@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Update Item", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Update Item");
        var evento = await scope.SeedEventoAsync("Evento Update Item");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(22));
        var voluntario1 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Voluntario Original", "original.update@app.com"), equipe, cargo);
        var voluntario2 = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Voluntario Novo", "novo.update@app.com"), equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario1, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var atualizado = await service.UpdateItemAsync(escala.Id, item.Id, new AtualizarEscalaItemDto
        {
            EquipeId = equipe.Id,
            CargoId = cargo.Id,
            VoluntarioId = voluntario2.Id,
            Ordem = 5
        }, liderUsuario.Id, false);

        atualizado.VoluntarioId.Should().Be(voluntario2.Id);
        atualizado.Ordem.Should().Be(5);

        var itemPersistido = await new EscalaRepository(scope.Context, scope.TenantContext).GetItemByIdAsync(item.Id);
        itemPersistido!.VoluntarioId.Should().Be(voluntario2.Id);
        itemPersistido.Ordem.Should().Be(5);
    }

    [Fact]
    public async Task DeleteItemAsync_WithRealRepositories_RemovesItemFromScale()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Delete Item", "lider.delete.item@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.delete.item@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Delete Item", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Delete Item");
        var evento = await scope.SeedEventoAsync("Evento Delete Item");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(23));
        var voluntario = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Voluntario Delete", "vol.delete@app.com"), equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        var item = await scope.SeedEscalaItemAsync(escala, equipe, cargo, voluntario, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        await service.DeleteItemAsync(escala.Id, item.Id, liderUsuario.Id, false);

        var itemPersistido = await new EscalaRepository(scope.Context, scope.TenantContext).GetItemByIdAsync(item.Id);
        itemPersistido.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithRealRepositories_CreatesDraftScaleWithObservations()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Criacao Escala", "lider.criacao.escala@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.criacao.escala@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Criacao Escala", liderUsuario.Id);
        var evento = await scope.SeedEventoAsync("Evento Criacao Escala");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(24));

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var criada = await service.CreateAsync(new CriarEscalaDto
        {
            EventoOcorrenciaId = ocorrencia.Id,
            EquipeId = equipe.Id,
            Observacoes = "Primeira versao da escala"
        }, liderUsuario.Id, false);

        criada.Status.Should().Be(StatusEscala.Rascunho);
        criada.EquipeId.Should().Be(equipe.Id);
        criada.Observacoes.Should().Be("Primeira versao da escala");

        var persistida = await new EscalaRepository(scope.Context, scope.TenantContext).GetByIdAsync(criada.Id);
        persistida.Should().NotBeNull();
        persistida!.Status.Should().Be(StatusEscala.Rascunho);
        persistida.Observacoes.Should().Be("Primeira versao da escala");
    }

    [Fact]
    public async Task CreateAsync_WithRealRepositories_ThrowsWhenScaleAlreadyExistsForOccurrenceAndTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Duplicidade Escala", "lider.duplicidade.escala@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.duplicidade.escala@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Duplicidade Escala", liderUsuario.Id);
        var evento = await scope.SeedEventoAsync("Evento Duplicidade Escala");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(25));
        await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.CreateAsync(new CriarEscalaDto
        {
            EventoOcorrenciaId = ocorrencia.Id,
            EquipeId = equipe.Id,
            Observacoes = "Nao deveria criar"
        }, liderUsuario.Id, false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Já existe escala*");
    }

    [Fact]
    public async Task UpdateAsync_WithRealRepositories_ChangesStatusAndObservations()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Update Escala", "lider.update.escala@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.update.escala@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Update Escala", liderUsuario.Id);
        var evento = await scope.SeedEventoAsync("Evento Update Escala");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(26));
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var atualizada = await service.UpdateAsync(escala.Id, new AtualizarEscalaDto
        {
            Status = StatusEscala.Publicada,
            Observacoes = "Escala revisada e pronta"
        }, liderUsuario.Id, false);

        atualizada.Status.Should().Be(StatusEscala.Publicada);
        atualizada.Observacoes.Should().Be("Escala revisada e pronta");

        var persistida = await new EscalaRepository(scope.Context, scope.TenantContext).GetByIdAsync(escala.Id);
        persistida.Should().NotBeNull();
        persistida!.Status.Should().Be(StatusEscala.Publicada);
        persistida.Observacoes.Should().Be("Escala revisada e pronta");
    }

    [Fact]
    public async Task DeleteAsync_WithRealRepositories_RemovesScale()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Delete Escala", "lider.delete.escala@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.delete.escala@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Delete Escala", liderUsuario.Id);
        var evento = await scope.SeedEventoAsync("Evento Delete Escala");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(27));
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        await service.DeleteAsync(escala.Id, liderUsuario.Id, false);

        var persistida = await new EscalaRepository(scope.Context, scope.TenantContext).GetByIdAsync(escala.Id);
        persistida.Should().BeNull();
    }

    [Fact]
    public async Task AddItemAsync_WithRealRepositories_ThrowsWhenScaleIsClosed()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Escala Fechada", "lider.escala.fechada@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.escala.fechada@app.com", TipoUsuario.Admin);
        var equipe = await scope.SeedEquipeAsync("Equipe Escala Fechada", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Escala Fechada");
        var evento = await scope.SeedEventoAsync("Evento Escala Fechada");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(28));
        var voluntario = await scope.SeedVoluntarioAsync(await scope.SeedPessoaAsync("Voluntario Escala Fechada", "vol.escala.fechada@app.com"), equipe, cargo);
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);
        escala.Status = StatusEscala.Fechada;
        await scope.Context.SaveChangesAsync();

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.AddItemAsync(escala.Id, new CriarEscalaItemDto
        {
            EquipeId = equipe.Id,
            CargoId = cargo.Id,
            VoluntarioId = voluntario.Id
        }, liderUsuario.Id, false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Escala fechada*");
    }

    [Fact]
    public async Task CreateAsync_WithRealRepositories_ThrowsWhenUserDoesNotManageTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Permissao Criacao", "lider.permissao.criacao@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.permissao.criacao@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Usuario Sem Gestao Criacao", "usuario.sem.gestao.criacao@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "usuario.sem.gestao.criacao@app.com", TipoUsuario.Portal);
        var equipe = await scope.SeedEquipeAsync("Equipe Permissao Criacao", liderUsuario.Id);
        var evento = await scope.SeedEventoAsync("Evento Permissao Criacao");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(29));

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.CreateAsync(new CriarEscalaDto
        {
            EventoOcorrenciaId = ocorrencia.Id,
            EquipeId = equipe.Id,
            Observacoes = "Sem permissao"
        }, outroUsuario.Id, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar escalas desta equipe*");
    }

    [Fact]
    public async Task DeleteAsync_WithRealRepositories_ThrowsWhenUserDoesNotManageTeam()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Permissao Delete", "lider.permissao.delete@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.permissao.delete@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Usuario Sem Gestao Delete", "usuario.sem.gestao.delete@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "usuario.sem.gestao.delete@app.com", TipoUsuario.Portal);
        var equipe = await scope.SeedEquipeAsync("Equipe Permissao Delete", liderUsuario.Id);
        var evento = await scope.SeedEventoAsync("Evento Permissao Delete");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(30));
        var escala = await scope.SeedEscalaAsync(ocorrencia, equipe, liderUsuario);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.DeleteAsync(escala.Id, outroUsuario.Id, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*gerenciar escalas desta equipe*");
    }

    [Fact]
    public async Task AddItemAsync_WithRealRepositories_ThrowsWhenVolunteerAlreadyHasConflictAndNoOverride()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Conflito Item", "lider.conflito.item@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.conflito.item@app.com", TipoUsuario.Admin);
        var equipeA = await scope.SeedEquipeAsync("Equipe Conflito A", liderUsuario.Id);
        var equipeB = await scope.SeedEquipeAsync("Equipe Conflito B", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Conflito Item");
        var evento = await scope.SeedEventoAsync("Evento Conflito Item");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(31));
        var pessoaCompartilhada = await scope.SeedPessoaAsync("Pessoa Conflito Item", "pessoa.conflito.item@app.com");
        var voluntarioA = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeA, cargo);
        var voluntarioB = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeB, cargo);
        var escalaA = await scope.SeedEscalaAsync(ocorrencia, equipeA, liderUsuario);
        var escalaB = await scope.SeedEscalaAsync(ocorrencia, equipeB, liderUsuario);
        await scope.SeedEscalaItemAsync(escalaA, equipeA, cargo, voluntarioA, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.AddItemAsync(escalaB.Id, new CriarEscalaItemDto
        {
            EquipeId = equipeB.Id,
            CargoId = cargo.Id,
            VoluntarioId = voluntarioB.Id
        }, liderUsuario.Id, false);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*já está escalado neste evento*");
    }

    [Fact]
    public async Task AddItemAsync_WithRealRepositories_ThrowsWhenNonAdminTriesToForceConflict()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Lider Override Conflito", "lider.override.conflito@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "lider.override.conflito@app.com", TipoUsuario.Admin);
        var outroPessoa = await scope.SeedPessoaAsync("Lider Sem Admin Override", "lider.sem.admin.override@app.com");
        var outroUsuario = await scope.SeedUsuarioAsync(outroPessoa, "lider.sem.admin.override@app.com", TipoUsuario.Portal);
        var equipeA = await scope.SeedEquipeAsync("Equipe Override A", outroUsuario.Id);
        var equipeB = await scope.SeedEquipeAsync("Equipe Override B", outroUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Override Conflito");
        var evento = await scope.SeedEventoAsync("Evento Override Conflito");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(32));
        var pessoaCompartilhada = await scope.SeedPessoaAsync("Pessoa Override", "pessoa.override@app.com");
        var voluntarioA = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeA, cargo);
        var voluntarioB = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeB, cargo);
        var escalaA = await scope.SeedEscalaAsync(ocorrencia, equipeA, outroUsuario);
        var escalaB = await scope.SeedEscalaAsync(ocorrencia, equipeB, outroUsuario);
        await scope.SeedEscalaItemAsync(escalaA, equipeA, cargo, voluntarioA, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.AddItemAsync(escalaB.Id, new CriarEscalaItemDto
        {
            EquipeId = equipeB.Id,
            CargoId = cargo.Id,
            VoluntarioId = voluntarioB.Id,
            ForcarConflito = true,
            MotivoExcecao = "Precisa servir"
        }, outroUsuario.Id, false);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Apenas administradores*");
    }

    [Fact]
    public async Task AddItemAsync_WithRealRepositories_ThrowsWhenAdminForcesConflictWithoutReason()
    {
        await using var scope = await IntegrationScope.CreateAsync();

        var liderPessoa = await scope.SeedPessoaAsync("Admin Override Sem Motivo", "admin.override.sem.motivo@app.com");
        var liderUsuario = await scope.SeedUsuarioAsync(liderPessoa, "admin.override.sem.motivo@app.com", TipoUsuario.Admin);
        var equipeA = await scope.SeedEquipeAsync("Equipe Sem Motivo A", liderUsuario.Id);
        var equipeB = await scope.SeedEquipeAsync("Equipe Sem Motivo B", liderUsuario.Id);
        var cargo = await scope.SeedCargoAsync("Sem Motivo");
        var evento = await scope.SeedEventoAsync("Evento Sem Motivo");
        var ocorrencia = await scope.SeedOcorrenciaAsync(evento, DateTime.Now.AddDays(33));
        var pessoaCompartilhada = await scope.SeedPessoaAsync("Pessoa Sem Motivo", "pessoa.sem.motivo@app.com");
        var voluntarioA = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeA, cargo);
        var voluntarioB = await scope.SeedVoluntarioAsync(pessoaCompartilhada, equipeB, cargo);
        var escalaA = await scope.SeedEscalaAsync(ocorrencia, equipeA, liderUsuario);
        var escalaB = await scope.SeedEscalaAsync(ocorrencia, equipeB, liderUsuario);
        await scope.SeedEscalaItemAsync(escalaA, equipeA, cargo, voluntarioA, StatusEscalaItem.Pendente);

        var service = new EscalaService(
            new EscalaRepository(scope.Context, scope.TenantContext),
            new EventoOcorrenciaRepository(scope.Context, scope.TenantContext),
            new VoluntarioRepository(scope.Context, scope.TenantContext),
            new EscalaModeloRepository(scope.Context, scope.TenantContext),
            new IndisponibilidadeVoluntarioRepository(scope.Context, scope.TenantContext),
            new EquipeRepository(scope.Context, scope.TenantContext),
            new UsuarioRepository(scope.Context, scope.TenantContext),
            CreateNotificationMock().Object,
            CreateComunicacaoMock().Object,
            Mock.Of<ILogger<EscalaService>>(),
            Mock.Of<IAuditLogService>(),
            scope.TenantContext);

        var act = () => service.AddItemAsync(escalaB.Id, new CriarEscalaItemDto
        {
            EquipeId = equipeB.Id,
            CargoId = cargo.Id,
            VoluntarioId = voluntarioB.Id,
            ForcarConflito = true
        }, liderUsuario.Id, true);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Motivo da exceção é obrigatório*");
    }

    private static IConfiguration CreateJwtConfiguration()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "uma-chave-super-segura-com-pelo-menos-32-caracteres",
                ["Jwt:Issuer"] = "SistemaIgreja.IntegrationTests",
                ["Jwt:Audience"] = "SistemaIgreja.IntegrationTests"
            })
            .Build();
    }

    private static Mock<INotificacaoUsuarioService> CreateNotificationMock()
    {
        var mock = new Mock<INotificacaoUsuarioService>();
        mock.Setup(x => x.CriarAsync(It.IsAny<CriarNotificacaoUsuarioDto>()))
            .Returns(Task.CompletedTask);
        mock.Setup(x => x.CriarParaUsuariosAsync(It.IsAny<IEnumerable<CriarNotificacaoUsuarioDto>>()))
            .Returns(Task.CompletedTask);
        return mock;
    }

    private static Mock<IComunicacaoAutomacaoService> CreateComunicacaoMock()
    {
        var mock = new Mock<IComunicacaoAutomacaoService>();
        mock.Setup(x => x.ExecutarLembretesOperacionaisAsync(It.IsAny<IEnumerable<ComunicacaoLembreteOperacionalRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        return mock;
    }

    private sealed class IntegrationScope : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        public SistemaIgrejaDbContext Context { get; }
        public ITenantContext TenantContext { get; }

        private IntegrationScope(SqliteConnection connection, SistemaIgrejaDbContext context, ITenantContext tenantContext)
        {
            _connection = connection;
            Context = context;
            TenantContext = tenantContext;
        }

        public static async Task<IntegrationScope> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var tenantContext = new FixedTenantContext(Tenant.InitialTenantId);
            var options = new DbContextOptionsBuilder<SistemaIgrejaDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new SistemaIgrejaDbContext(options, tenantContext);
            await context.Database.EnsureCreatedAsync();

            return new IntegrationScope(connection, context, tenantContext);
        }

        public async Task<Pessoa> SeedPessoaAsync(string nome, string email)
        {
            var pessoa = new Pessoa
            {
                TenantId = Tenant.InitialTenantId,
                Nome = nome,
                Email = email,
                TipoPessoa = TipoPessoa.Adulto,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            Context.Pessoas.Add(pessoa);
            await Context.SaveChangesAsync();
            return pessoa;
        }

        public async Task<PerfilAcesso> SeedPerfilAcessoAsync(string nome, string recurso, bool podeVer, bool podeEditar, bool podeExcluir)
        {
            var perfil = new PerfilAcesso
            {
                TenantId = Tenant.InitialTenantId,
                Nome = nome,
                DataCriacao = DateTime.Now,
                Permissoes =
                [
                    new PerfilAcessoPermissao
                    {
                        TenantId = Tenant.InitialTenantId,
                        Recurso = recurso,
                        PodeVer = podeVer,
                        PodeEditar = podeEditar,
                        PodeExcluir = podeExcluir
                    }
                ]
            };

            Context.PerfisAcesso.Add(perfil);
            await Context.SaveChangesAsync();
            return perfil;
        }

        public async Task<Usuario> SeedUsuarioAsync(Pessoa pessoa, string emailLogin, TipoUsuario tipoUsuario, PerfilAcesso? perfil = null, bool ativo = true, bool isPlatformAdmin = false, string senha = "123456")
        {
            var usuario = new Usuario
            {
                TenantId = Tenant.InitialTenantId,
                PessoaId = pessoa.Id,
                EmailLogin = emailLogin,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
                TipoUsuario = tipoUsuario,
                Ativo = ativo,
                IsPlatformAdmin = isPlatformAdmin,
                PerfilAcessoId = perfil?.Id,
                DataCriacao = DateTime.Now
            };

            Context.Usuarios.Add(usuario);
            await Context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Equipe> SeedEquipeAsync(string nome, int? liderUsuarioId = null)
        {
            var equipe = new Equipe
            {
                TenantId = Tenant.InitialTenantId,
                Nome = nome,
                Area = AreaEquipe.Verde,
                LiderUsuarioId = liderUsuarioId,
                DataCriacao = DateTime.Now
            };

            Context.Equipes.Add(equipe);
            await Context.SaveChangesAsync();
            return equipe;
        }

        public async Task<Cargo> SeedCargoAsync(string nome)
        {
            var cargo = new Cargo
            {
                TenantId = Tenant.InitialTenantId,
                Nome = nome,
                DataCriacao = DateTime.Now
            };

            Context.Cargos.Add(cargo);
            await Context.SaveChangesAsync();
            return cargo;
        }

        public async Task<Evento> SeedEventoAsync(string titulo)
        {
            var evento = new Evento
            {
                TenantId = Tenant.InitialTenantId,
                Titulo = titulo,
                DataInicio = DateTime.Now.AddDays(1),
                DataFim = DateTime.Now.AddDays(1).AddHours(2),
                Tipo = TipoEvento.Culto,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            Context.Eventos.Add(evento);
            await Context.SaveChangesAsync();
            return evento;
        }

        public async Task<EventoOcorrencia> SeedOcorrenciaAsync(Evento evento, DateTime dataHoraInicio)
        {
            var ocorrencia = new EventoOcorrencia
            {
                TenantId = Tenant.InitialTenantId,
                EventoId = evento.Id,
                DataHoraInicio = dataHoraInicio,
                DataHoraFim = dataHoraInicio.AddHours(2),
                Status = StatusEventoOcorrencia.Confirmado,
                GeradaAutomaticamente = false,
                DataCriacao = DateTime.Now
            };

            Context.EventosOcorrencias.Add(ocorrencia);
            await Context.SaveChangesAsync();
            return ocorrencia;
        }

        public async Task<Voluntario> SeedVoluntarioAsync(Pessoa pessoa, Equipe equipe, Cargo cargo)
        {
            var voluntario = new Voluntario
            {
                TenantId = Tenant.InitialTenantId,
                PessoaId = pessoa.Id,
                EquipeId = equipe.Id,
                CargoId = cargo.Id,
                DataCadastro = DateTime.Now
            };

            Context.Voluntarios.Add(voluntario);
            await Context.SaveChangesAsync();
            return voluntario;
        }

        public async Task<Escala> SeedEscalaAsync(EventoOcorrencia ocorrencia, Equipe equipe, Usuario criador)
        {
            var escala = new Escala
            {
                TenantId = Tenant.InitialTenantId,
                EventoOcorrenciaId = ocorrencia.Id,
                EquipeId = equipe.Id,
                Status = StatusEscala.Rascunho,
                CriadoPorUsuarioId = criador.Id,
                DataCriacao = DateTime.Now
            };

            Context.Escalas.Add(escala);
            await Context.SaveChangesAsync();
            return escala;
        }

        public async Task<EscalaItem> SeedEscalaItemAsync(Escala escala, Equipe equipe, Cargo cargo, Voluntario voluntario, StatusEscalaItem status)
        {
            var item = new EscalaItem
            {
                TenantId = Tenant.InitialTenantId,
                EscalaId = escala.Id,
                EquipeId = equipe.Id,
                CargoId = cargo.Id,
                VoluntarioId = voluntario.Id,
                Ordem = 1,
                Status = status,
                DataConvite = DateTime.Now,
                DataCriacao = DateTime.Now
            };

            Context.EscalasItens.Add(item);
            await Context.SaveChangesAsync();
            return item;
        }

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class FixedTenantContext(int tenantId) : ITenantContext
    {
        public int? TenantId { get; } = tenantId;
        public string? TenantSlug { get; } = tenantId == Tenant.InitialTenantId ? Tenant.InitialTenantSlug : null;
        public bool IsResolved => TenantId.HasValue;
    }
}
