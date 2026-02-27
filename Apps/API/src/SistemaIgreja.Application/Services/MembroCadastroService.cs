using SistemaIgreja.Application.DTOs;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Domain.Entities;

namespace SistemaIgreja.Application.Services;

public interface IMembroCadastroService
{
    Task<CadastroMembroResultadoDto> CadastrarAsync(CadastroMembroDto dto);
}

public class CadastroMembroResultadoDto
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int? PessoaId { get; set; }
}

public class MembroCadastroService : IMembroCadastroService
{
    private readonly IPessoaRepository _pessoaRepository;
    private readonly IPessoaPerfilRepository _perfilRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MembroCadastroService(
        IPessoaRepository pessoaRepository,
        IPessoaPerfilRepository perfilRepository,
        IUnitOfWork unitOfWork)
    {
        _pessoaRepository = pessoaRepository;
        _perfilRepository = perfilRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CadastroMembroResultadoDto> CadastrarAsync(CadastroMembroDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return new CadastroMembroResultadoDto { Sucesso = false, Mensagem = "Nome é obrigatório" };

        var whatsAppNormalizado = NormalizarTelefone(dto.WhatsApp);
        if (string.IsNullOrWhiteSpace(whatsAppNormalizado) || whatsAppNormalizado.Length < 10)
            return new CadastroMembroResultadoDto { Sucesso = false, Mensagem = "WhatsApp inválido. Informe um número com DDD." };

        if (string.IsNullOrWhiteSpace(dto.Email))
            return new CadastroMembroResultadoDto { Sucesso = false, Mensagem = "Email é obrigatório" };
        if (!IsValidEmail(dto.Email))
            return new CadastroMembroResultadoDto { Sucesso = false, Mensagem = "Email inválido" };

        var pessoaId = 0;

        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // 1. Buscar pessoa existente por email ou whatsapp
            Pessoa? pessoa = await _pessoaRepository.GetByEmailAsync(dto.Email);
            if (pessoa == null && !string.IsNullOrWhiteSpace(whatsAppNormalizado))
            {
                pessoa = await _pessoaRepository.GetByWhatsAppAsync(whatsAppNormalizado);
            }

            // 2. Criar ou atualizar Pessoa
            if (pessoa == null)
            {
                pessoa = new Pessoa
                {
                    Nome = dto.Nome.Trim(),
                    Email = dto.Email.Trim(),
                    WhatsApp = whatsAppNormalizado,
                    DataNascimento = dto.DataNascimento,
                    TipoPessoa = TipoPessoa.Adulto,
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
                };
                pessoa = await _pessoaRepository.CreateWithoutSaveAsync(pessoa);
            }
            else
            {
                // Atualizar campos vazios sem sobrescrever
                var atualizado = false;
                if (string.IsNullOrWhiteSpace(pessoa.Nome) && !string.IsNullOrWhiteSpace(dto.Nome))
                {
                    pessoa.Nome = dto.Nome.Trim();
                    atualizado = true;
                }
                if (string.IsNullOrWhiteSpace(pessoa.Email) && !string.IsNullOrWhiteSpace(dto.Email))
                {
                    pessoa.Email = dto.Email.Trim();
                    atualizado = true;
                }
                if (string.IsNullOrWhiteSpace(pessoa.WhatsApp) && !string.IsNullOrWhiteSpace(whatsAppNormalizado))
                {
                    pessoa.WhatsApp = whatsAppNormalizado;
                    atualizado = true;
                }
                if (pessoa.DataNascimento == null && dto.DataNascimento.HasValue)
                {
                    pessoa.DataNascimento = dto.DataNascimento;
                    atualizado = true;
                }
                if (atualizado)
                {
                    await _pessoaRepository.UpdateWithoutSaveAsync(pessoa);
                }
            }

            pessoaId = pessoa.Id;

            // 3. Garantir perfil Membro
            var perfilMembro = await _perfilRepository.GetPerfilAtivoAsync(pessoa.Id, PerfilPessoa.Membro);
            if (perfilMembro == null)
            {
                var novoPerfil = new PessoaPerfil
                {
                    PessoaId = pessoa.Id,
                    Perfil = PerfilPessoa.Membro,
                    DataInicio = DateTime.UtcNow,
                    DataFim = null
                };
                await _perfilRepository.CreateWithoutSaveAsync(novoPerfil);
            }

            await _unitOfWork.SaveChangesAsync();
        });

        return new CadastroMembroResultadoDto
        {
            Sucesso = true,
            Mensagem = "Cadastro realizado com sucesso!",
            PessoaId = pessoaId
        };
    }

    private static string NormalizarTelefone(string? telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            return string.Empty;
        return new string(telefone.Where(char.IsDigit).ToArray());
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email.Trim();
        }
        catch
        {
            return false;
        }
    }
}
