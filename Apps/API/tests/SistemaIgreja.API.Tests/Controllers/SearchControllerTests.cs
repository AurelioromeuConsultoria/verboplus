using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SistemaIgreja.API.Controllers;
using SistemaIgreja.Application.DTOs.Search;
using SistemaIgreja.Application.Services;

namespace SistemaIgreja.API.Tests.Controllers;

public class SearchControllerTests
{
    private readonly Mock<ISearchService> _serviceMock = new();
    private readonly SearchController _controller;

    public SearchControllerTests()
    {
        _controller = new SearchController(_serviceMock.Object);
    }

    [Fact]
    public async Task Search_ReturnsEmpty_WhenQueryIsTooShort()
    {
        var result = await _controller.Search("a");

        result.Result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new GlobalSearchResultDto { Items = Array.Empty<GlobalSearchItemDto>() });
    }

    [Fact]
    public async Task Search_ReturnsOk_WhenServiceSucceeds()
    {
        _serviceMock.Setup(s => s.SearchAsync("marco", 10))
            .ReturnsAsync(new List<GlobalSearchItemDto> { new() { Type = "Pessoa", Id = 1, Title = "Marco" } });

        var result = await _controller.Search("marco", 10);

        result.Result.Should().BeOfType<OkObjectResult>();
    }
}
