using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaIgreja.Application.DTOs.Search;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly ISearchService _service;

    public SearchController(ISearchService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<GlobalSearchResultDto>> Search([FromQuery] string q, [FromQuery] int limit = 20)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
        {
            return Ok(new GlobalSearchResultDto { Items = Array.Empty<GlobalSearchItemDto>() });
        }

        var items = await _service.SearchAsync(q, limit);
        return Ok(new GlobalSearchResultDto { Items = items });
    }
}

