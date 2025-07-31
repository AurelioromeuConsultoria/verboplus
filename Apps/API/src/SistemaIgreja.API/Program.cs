using Microsoft.EntityFrameworkCore;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Infrastructure.Repositories;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SistemaIgrejaDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositórios
builder.Services.AddScoped<IVisitanteRepository, VisitanteRepository>();
builder.Services.AddScoped<IConfiguracaoMensagemRepository, ConfiguracaoMensagemRepository>();
builder.Services.AddScoped<IMensagemAgendadaRepository, MensagemAgendadaRepository>();

// Serviços
builder.Services.AddScoped<IVisitanteService, VisitanteService>();
builder.Services.AddScoped<IConfiguracaoMensagemService, ConfiguracaoMensagemService>();
builder.Services.AddScoped<IMensagemAgendadaService, MensagemAgendadaService>();

// Serviço de agendamento
builder.Services.AddHostedService<MessageSchedulerService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:5173", "http://localhost:3000", "http://localhost:4173" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
// Removido app.UseHttpsRedirection() para evitar redirecionamento forçado
app.UseAuthorization();
app.MapControllers();

// Aplicar migrations automaticamente em desenvolvimento
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SistemaIgrejaDbContext>();
    context.Database.EnsureCreated();
}

app.Run("http://localhost:5000");

