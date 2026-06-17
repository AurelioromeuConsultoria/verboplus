using FluentAssertions;
using SistemaIgreja.Application.Security;

namespace SistemaIgreja.API.Tests.Services;

public class PasswordPolicyTests
{
    [Theory]
    [InlineData("Senha123")]
    [InlineData("Abc12345")]
    [InlineData("UmaSenhaForte9")]
    public void Avaliar_RetornaNull_QuandoSenhaValida(string senha)
    {
        PasswordPolicy.Avaliar(senha).Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Ab1")]        // curta
    [InlineData("Senha12")]    // 7 caracteres
    public void Avaliar_ExigeComprimentoMinimo(string? senha)
    {
        PasswordPolicy.Avaliar(senha).Should().Contain("8 caracteres");
    }

    [Theory]
    [InlineData("semmaiuscula9")]   // sem maiúscula
    [InlineData("SEMMINUSCULA9")]   // sem minúscula
    [InlineData("SemNumeroAqui")]   // sem número
    public void Avaliar_ExigeComplexidade(string senha)
    {
        PasswordPolicy.Avaliar(senha).Should().Contain("maiúscula");
    }

    [Fact]
    public void Validar_LancaArgumentException_QuandoSenhaFraca()
    {
        var act = () => PasswordPolicy.Validar("fraca");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validar_NaoLanca_QuandoSenhaForte()
    {
        var act = () => PasswordPolicy.Validar("SenhaForte1");
        act.Should().NotThrow();
    }
}
