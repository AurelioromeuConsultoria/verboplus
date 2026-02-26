using System.Security.Claims;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Services;

public class HttpCurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _http;

    public HttpCurrentUserContext(IHttpContextAccessor http)
    {
        _http = http;
    }

    public int? UserId
    {
        get
        {
            var ctx = _http.HttpContext;
            var raw = ctx?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) && id > 0 ? id : null;
        }
    }

    public string? UserName => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
    public string? UserEmail => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
    public string? IpAddress => _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();
}

