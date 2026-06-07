using FluentAssertions;
using Moq;
using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.API.Tests.Services;

public class KidsConteudoAulaServiceTests
{
    private readonly Mock<IKidsConteudoAulaRepository> _conteudoRepository = new();
    private readonly Mock<IKidsConteudoAulaAnexoRepository> _anexoRepository = new();
    private readonly Mock<IKidsEstruturaRepository> _estruturaRepository = new();
    private readonly Mock<IEventoOcorrenciaRepository> _eventoOcorrenciaRepository = new();
    private readonly Mock<IKidsAuthorizationService> _authorizationService = new();
    private readonly Mock<IResponsavelCriancaRepository> _responsavelCriancaRepository = new();
    private readonly Mock<ICriancaDetalheRepository> _criancaDetalheRepository = new();
    private readonly Mock<IPessoaRepository> _pessoaRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private readonly KidsConteudoAulaService _service;

    public KidsConteudoAulaServiceTests()
    {
        _authorizationService.Setup(a => a.EnsureOperadorAsync()).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.EnsureLiderAsync()).Returns(Task.CompletedTask);
        _authorizationService.Setup(a => a.GetCurrentContextAsync())
            .ReturnsAsync(new KidsAuthorizationContext
            {
                UsuarioId = 15,
                PessoaId = 30,
                TenantId = 1,
                TipoUsuario = TipoUsuario.Admin,
                PerfisAtivos = Array.Empty<PerfilPessoa>()
            });

        _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<KidsConteudoAula>>>())).Returns<Func<Task<KidsConteudoAula>>>(f => f());
        _unitOfWork.Setup(u => u.ExecuteInTransactionAsync(It.IsAny<Func<Task<KidsConteudoAulaAdminDto>>>())).Returns<Func<Task<KidsConteudoAulaAdminDto>>>(f => f());

        _service = new KidsConteudoAulaService(
            _conteudoRepository.Object,
            _anexoRepository.Object,
            _estruturaRepository.Object,
            _eventoOcorrenciaRepository.Object,
            _authorizationService.Object,
            _responsavelCriancaRepository.Object,
            _criancaDetalheRepository.Object,
            _pessoaRepository.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task CreateAsync_DeveCriarDraftComAnexos()
    {
        _estruturaRepository.Setup(r => r.GetSalaByIdAsync("SALA-1"))
            .ReturnsAsync(new KidsSala { Id = "SALA-1", Nome = "Sala 1", Ativo = true, DataCriacao = DateTime.UtcNow });
        _estruturaRepository.Setup(r => r.GetTurmaByIdAsync("TURMA-1"))
            .ReturnsAsync(new KidsTurma { Id = "TURMA-1", SalaId = "SALA-1", Nome = "Turma 1", Ativo = true, DataCriacao = DateTime.UtcNow });
        _eventoOcorrenciaRepository.Setup(r => r.ExistsAsync(22)).ReturnsAsync(true);

        _conteudoRepository.Setup(r => r.CreateWithoutSaveAsync(It.IsAny<KidsConteudoAula>()))
            .ReturnsAsync((KidsConteudoAula item) =>
            {
                item.Id = 91;
                return item;
            });
        _conteudoRepository.Setup(r => r.GetByIdAsync(91))
            .ReturnsAsync(new KidsConteudoAula
            {
                Id = 91,
                Titulo = "Lição do domingo",
                Resumo = "Resumo da aula",
                Tema = "Fé",
                Versiculo = "João 3:16",
                AtividadeEmCasa = "Colorir",
                ObservacaoResponsavel = "Conversar em casa",
                Status = "Draft",
                DataReferencia = new DateTime(2026, 5, 6),
                EventoOcorrenciaId = 22,
                SalaId = "SALA-1",
                TurmaId = "TURMA-1",
                CriadoEm = DateTime.UtcNow,
                Anexos = new List<KidsConteudoAulaAnexo>
                {
                    new()
                    {
                        Id = 5,
                        ConteudoAulaId = 91,
                        Tipo = "Pdf",
                        NomeExibicao = "Atividade",
                        Url = "https://cdn.local/atividade.pdf",
                        Ordem = 1,
                        CriadoEm = DateTime.UtcNow
                    }
                }
            });

        var result = await _service.CreateAsync(new CreateKidsConteudoAulaRequest
        {
            Titulo = "Lição do domingo",
            Tema = "Fé",
            Versiculo = "João 3:16",
            Resumo = "Resumo da aula",
            AtividadeEmCasa = "Colorir",
            ObservacaoResponsavel = "Conversar em casa",
            DataReferencia = new DateTime(2026, 5, 6),
            EventoOcorrenciaId = 22,
            SalaId = "sala-1",
            TurmaId = "turma-1",
            Anexos =
            {
                new CreateKidsConteudoAulaAnexoRequest
                {
                    Tipo = "pdf",
                    NomeExibicao = "Atividade",
                    Url = "https://cdn.local/atividade.pdf",
                    Ordem = 1
                }
            }
        });

        result.Id.Should().Be(91);
        result.Status.Should().Be("Draft");
        result.SalaId.Should().Be("SALA-1");
        result.TurmaId.Should().Be("TURMA-1");
        result.Anexos.Should().ContainSingle();

        _conteudoRepository.Verify(r => r.CreateWithoutSaveAsync(It.Is<KidsConteudoAula>(item =>
            item.Titulo == "Lição do domingo" &&
            item.Status == "Draft" &&
            item.SalaId == "SALA-1" &&
            item.TurmaId == "TURMA-1")), Times.Once);

        _anexoRepository.Verify(r => r.CreateRangeWithoutSaveAsync(It.Is<IEnumerable<KidsConteudoAulaAnexo>>(items =>
            items.Count() == 1 &&
            items.First().ConteudoAulaId == 91 &&
            items.First().Tipo == "Pdf")), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DeveBloquearQuandoTurmaNaoPertencerASala()
    {
        _estruturaRepository.Setup(r => r.GetSalaByIdAsync("SALA-1"))
            .ReturnsAsync(new KidsSala { Id = "SALA-1", Nome = "Sala 1", Ativo = true, DataCriacao = DateTime.UtcNow });
        _estruturaRepository.Setup(r => r.GetTurmaByIdAsync("TURMA-X"))
            .ReturnsAsync(new KidsTurma { Id = "TURMA-X", SalaId = "SALA-2", Nome = "Turma X", Ativo = true, DataCriacao = DateTime.UtcNow });

        await _service.Invoking(s => s.CreateAsync(new CreateKidsConteudoAulaRequest
        {
            Titulo = "Teste",
            Resumo = "Resumo",
            DataReferencia = new DateTime(2026, 5, 6),
            SalaId = "SALA-1",
            TurmaId = "TURMA-X"
        }))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*não pertence à sala*");
    }

    [Fact]
    public async Task PublicarAsync_DevePublicarConteudo()
    {
        var item = new KidsConteudoAula
        {
            Id = 11,
            Titulo = "Aula 1",
            Resumo = "Resumo",
            Status = "Draft",
            DataReferencia = new DateTime(2026, 5, 6),
            CriadoEm = DateTime.UtcNow
        };

        _conteudoRepository.SetupSequence(r => r.GetByIdAsync(11))
            .ReturnsAsync(item)
            .ReturnsAsync(new KidsConteudoAula
            {
                Id = 11,
                Titulo = "Aula 1",
                Resumo = "Resumo",
                Status = "Published",
                DataReferencia = item.DataReferencia,
                CriadoEm = item.CriadoEm,
                PublicadoEm = DateTime.UtcNow,
                PublicadoPorPessoaId = 30,
                PublicadoPor = new Pessoa { Id = 30, Nome = "Marco", TipoPessoa = TipoPessoa.Adulto, Ativo = true, DataCriacao = DateTime.UtcNow }
            });

        var result = await _service.PublicarAsync(11);

        result.Status.Should().Be("Published");
        result.PublicadoPorPessoaId.Should().Be(30);
        _conteudoRepository.Verify(r => r.UpdateWithoutSaveAsync(It.Is<KidsConteudoAula>(c =>
            c.Id == 11 &&
            c.Status == "Published" &&
            c.PublicadoPorPessoaId == 30 &&
            c.PublicadoEm.HasValue)), Times.Once);
    }

    [Fact]
    public async Task ArquivarAsync_DeveArquivarConteudo()
    {
        var item = new KidsConteudoAula
        {
            Id = 19,
            Titulo = "Aula 2",
            Resumo = "Resumo",
            Status = "Published",
            DataReferencia = new DateTime(2026, 5, 6),
            CriadoEm = DateTime.UtcNow
        };

        _conteudoRepository.SetupSequence(r => r.GetByIdAsync(19))
            .ReturnsAsync(item)
            .ReturnsAsync(new KidsConteudoAula
            {
                Id = 19,
                Titulo = "Aula 2",
                Resumo = "Resumo",
                Status = "Archived",
                DataReferencia = item.DataReferencia,
                CriadoEm = item.CriadoEm
            });

        var result = await _service.ArquivarAsync(19);

        result.Status.Should().Be("Archived");
        _conteudoRepository.Verify(r => r.UpdateWithoutSaveAsync(It.Is<KidsConteudoAula>(c =>
            c.Id == 19 &&
            c.Status == "Archived" &&
            c.AtualizadoEm.HasValue)), Times.Once);
    }

    [Fact]
    public async Task GetMeuConteudoPorCriancaAsync_DeveFiltrarPorSalaETurma()
    {
        _responsavelCriancaRepository.Setup(r => r.ExisteVinculoAtivoAsync(10, 30)).ReturnsAsync(true);
        _criancaDetalheRepository.Setup(r => r.GetByPessoaIdAsync(10))
            .ReturnsAsync(new CriancaDetalhe
            {
                PessoaId = 10,
                SalaId = "SALA-1",
                TurmaId = "TURMA-1",
                DataCadastro = DateTime.UtcNow
            });
        _pessoaRepository.Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Pessoa { Id = 10, Nome = "Arthur", TipoPessoa = TipoPessoa.Crianca, Ativo = true, DataCriacao = DateTime.UtcNow });
        _conteudoRepository.Setup(r => r.GetAllAsync("Published", null, null, null, 10))
            .ReturnsAsync(new List<KidsConteudoAula>
            {
                new()
                {
                    Id = 1,
                    Titulo = "Geral",
                    Resumo = "Todos recebem",
                    Status = "Published",
                    DataReferencia = new DateTime(2026, 5, 6),
                    CriadoEm = DateTime.UtcNow
                },
                new()
                {
                    Id = 2,
                    Titulo = "Turma alvo",
                    Resumo = "Turma correta",
                    Status = "Published",
                    SalaId = "SALA-1",
                    TurmaId = "TURMA-1",
                    DataReferencia = new DateTime(2026, 5, 6),
                    CriadoEm = DateTime.UtcNow
                },
                new()
                {
                    Id = 3,
                    Titulo = "Outra turma",
                    Resumo = "Não deve aparecer",
                    Status = "Published",
                    SalaId = "SALA-1",
                    TurmaId = "TURMA-X",
                    DataReferencia = new DateTime(2026, 5, 6),
                    CriadoEm = DateTime.UtcNow
                }
            });

        var result = await _service.GetMeuConteudoPorCriancaAsync(10, 10);

        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().BeEquivalentTo(new[] { 1, 2 });
    }
}
