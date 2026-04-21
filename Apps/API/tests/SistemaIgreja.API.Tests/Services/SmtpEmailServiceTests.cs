using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Infrastructure.Services;

namespace SistemaIgreja.API.Tests.Services;

public class SmtpEmailServiceTests
{
    [Fact]
    public async Task SendAsync_ThrowsWhenRecipientIsMissing()
    {
        var service = CreateService(new EmailSettings { Enabled = false });

        await Assert.ThrowsAsync<ArgumentException>(() => service.SendAsync(new EmailMessage
        {
            To = "",
            Subject = "Assunto",
            TextBody = "Texto"
        }));
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenSubjectIsMissing()
    {
        var service = CreateService(new EmailSettings { Enabled = false });

        await Assert.ThrowsAsync<ArgumentException>(() => service.SendAsync(new EmailMessage
        {
            To = "destino@app.com",
            Subject = "",
            TextBody = "Texto"
        }));
    }

    [Fact]
    public async Task SendAsync_ReturnsWhenDisabled()
    {
        var service = CreateService(new EmailSettings { Enabled = false });

        var act = async () => await service.SendAsync(new EmailMessage
        {
            To = "destino@app.com",
            Subject = "Assunto",
            TextBody = "Texto"
        });

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendAsync_ThrowsWhenEnabledAndConfigurationIsInvalid()
    {
        var service = CreateService(new EmailSettings
        {
            Enabled = true,
            Host = "",
            FromAddress = ""
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SendAsync(new EmailMessage
        {
            To = "destino@app.com",
            Subject = "Assunto",
            TextBody = "Texto"
        }));
    }

    private static SmtpEmailService CreateService(EmailSettings settings)
    {
        return new SmtpEmailService(
            Options.Create(settings),
            NullLogger<SmtpEmailService>.Instance);
    }
}
