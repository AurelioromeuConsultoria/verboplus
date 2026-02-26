namespace SistemaIgreja.Application.Services;

public interface ICurrentUserContext
{
    int? UserId { get; }
    string? UserName { get; }
    string? UserEmail { get; }
    string? IpAddress { get; }
}

