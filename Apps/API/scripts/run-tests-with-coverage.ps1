Param(
    [string]$TestProjectPath = "tests/SistemaIgreja.API.Tests/SistemaIgreja.API.Tests.csproj"
)

$ErrorActionPreference = 'Stop'

Write-Host "Running tests with coverage for: $TestProjectPath" -ForegroundColor Cyan

dotnet test $TestProjectPath --collect:"XPlat Code Coverage"

Write-Host "Generating HTML coverage report..." -ForegroundColor Cyan

dotnet build $TestProjectPath -t:Coverage

$reportPath = Join-Path (Split-Path $TestProjectPath -Parent) "TestResults/CoverageReport/index.html"
Write-Host "Coverage report: $reportPath" -ForegroundColor Green
