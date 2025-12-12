using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Infrastructure.Repositories;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SistemaIgrejaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


// Repositórios
builder.Services.AddScoped<IVisitanteRepository, VisitanteRepository>();
builder.Services.AddScoped<IConfiguracaoMensagemRepository, ConfiguracaoMensagemRepository>();
builder.Services.AddScoped<IMensagemAgendadaRepository, MensagemAgendadaRepository>();
// Novos repositórios
builder.Services.AddScoped<IEquipeRepository, EquipeRepository>();
builder.Services.AddScoped<ICargoRepository, CargoRepository>();
builder.Services.AddScoped<IVoluntarioRepository, VoluntarioRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<IDestaqueSiteRepository, DestaqueSiteRepository>();
builder.Services.AddScoped<ICategoriaNoticiaRepository, CategoriaNoticiaRepository>();
builder.Services.AddScoped<INoticiaRepository, NoticiaRepository>();
builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<IInscricaoEventoRepository, InscricaoEventoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICategoriaMidiaRepository, CategoriaMidiaRepository>();
builder.Services.AddScoped<IGaleriaFotoRepository, GaleriaFotoRepository>();

// Serviços
builder.Services.AddScoped<IVisitanteService, VisitanteService>();
builder.Services.AddScoped<IConfiguracaoMensagemService, ConfiguracaoMensagemService>();
builder.Services.AddScoped<IMensagemAgendadaService, MensagemAgendadaService>();
// Novos serviços
builder.Services.AddScoped<IEquipeService, EquipeService>();
builder.Services.AddScoped<ICargoService, CargoService>();
builder.Services.AddScoped<IVoluntarioService, VoluntarioService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<IDestaqueSiteService, DestaqueSiteService>();
builder.Services.AddScoped<ICategoriaNoticiaService, CategoriaNoticiaService>();
builder.Services.AddScoped<INoticiaService, NoticiaService>();
builder.Services.AddScoped<IContatoService, ContatoService>();
builder.Services.AddScoped<IInscricaoEventoService, InscricaoEventoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoriaMidiaService, CategoriaMidiaService>();
builder.Services.AddScoped<IGaleriaFotoService, GaleriaFotoService>();

// Serviço de agendamento
builder.Services.AddHostedService<MessageSchedulerService>();

// Configurar JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não configurada");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "SistemaIgreja";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "SistemaIgreja";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema Igreja API", Version = "v1" });
    
    // Configurar JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
            ?? new[] { "http://localhost:5173", "http://localhost:5174", "http://localhost:3000", "http://localhost:4173" };
        
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// IMPORTANTE: UseCors deve vir ANTES de UseAuthentication, UseAuthorization e MapControllers
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Removido app.UseHttpsRedirection() para evitar redirecionamento forçado

// Servir arquivos estáticos da pasta wwwroot (padrão)
app.UseStaticFiles();

// Servir arquivos da pasta uploads (raiz do projeto)
var uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SistemaIgrejaDbContext>();
    context.Database.Migrate();
}

app.Run();

