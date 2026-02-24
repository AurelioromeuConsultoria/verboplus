using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Infrastructure.Repositories;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Infrastructure.Services;
using SistemaIgreja.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

// ==========================
// DATABASE CONFIGURATION
// ==========================

var databaseProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (databaseProvider.Equals("postgresql", StringComparison.OrdinalIgnoreCase) ||
    databaseProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
{
    AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
}

builder.Services.AddDbContext<SistemaIgrejaDbContext>(options =>
{
    switch (databaseProvider.ToLowerInvariant())
    {
        case "postgresql":
        case "postgres":
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.EnableRetryOnFailure());
            break;

        case "sqlserver":
        default:
            options.UseSqlServer(connectionString);
            break;
    }
});

// ==========================
// DEPENDENCY INJECTION
// ==========================

// Repositories
builder.Services.AddScoped<IPessoaRepository, PessoaRepository>();
builder.Services.AddScoped<IPessoaPerfilRepository, PessoaPerfilRepository>();
builder.Services.AddScoped<IVisitanteRepository, VisitanteRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IConfiguracaoMensagemRepository, ConfiguracaoMensagemRepository>();
builder.Services.AddScoped<IMensagemAgendadaRepository, MensagemAgendadaRepository>();
builder.Services.AddScoped<IEquipeRepository, EquipeRepository>();
builder.Services.AddScoped<IHubCasaRepository, HubCasaRepository>();
builder.Services.AddScoped<IFornecedorRepository, FornecedorRepository>();
builder.Services.AddScoped<ICargoRepository, CargoRepository>();
builder.Services.AddScoped<IVoluntarioRepository, VoluntarioRepository>();
builder.Services.AddScoped<IEventoRepository, EventoRepository>();
builder.Services.AddScoped<IDestaqueSiteRepository, DestaqueSiteRepository>();
builder.Services.AddScoped<ICategoriaNoticiaRepository, CategoriaNoticiaRepository>();
builder.Services.AddScoped<INoticiaRepository, NoticiaRepository>();
builder.Services.AddScoped<IContatoRepository, ContatoRepository>();
builder.Services.AddScoped<IInscricaoEventoRepository, InscricaoEventoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPerfilAcessoRepository, PerfilAcessoRepository>();
builder.Services.AddScoped<ICategoriaMidiaRepository, CategoriaMidiaRepository>();
builder.Services.AddScoped<IGaleriaFotoRepository, GaleriaFotoRepository>();
builder.Services.AddScoped<IEnqueteRepository, EnqueteRepository>();
builder.Services.AddScoped<IConfiguracaoPortalRepository, ConfiguracaoPortalRepository>();

// Kids
builder.Services.AddScoped<ICriancaDetalheRepository, CriancaDetalheRepository>();
builder.Services.AddScoped<IResponsavelCriancaRepository, ResponsavelCriancaRepository>();
builder.Services.AddScoped<IKidsCheckinRepository, KidsCheckinRepository>();
builder.Services.AddScoped<IKidsNotificacaoRepository, KidsNotificacaoRepository>();

// Services
builder.Services.AddScoped<IPessoaService, PessoaService>();
builder.Services.AddScoped<IPessoaPerfilService, PessoaPerfilService>();
builder.Services.AddScoped<IVisitanteService, VisitanteService>();
builder.Services.AddScoped<IConfiguracaoMensagemService, ConfiguracaoMensagemService>();
builder.Services.AddScoped<IMensagemAgendadaService, MensagemAgendadaService>();
builder.Services.AddScoped<IEquipeService, EquipeService>();
builder.Services.AddScoped<IHubCasaService, HubCasaService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
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
builder.Services.AddScoped<IPerfilAcessoService, PerfilAcessoService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<ICategoriaMidiaService, CategoriaMidiaService>();
builder.Services.AddScoped<IGaleriaFotoService, GaleriaFotoService>();
builder.Services.AddScoped<IEnqueteService, EnqueteService>();
builder.Services.AddScoped<IConfiguracaoPortalService, ConfiguracaoPortalService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IKidsService, KidsService>();

builder.Services.Configure<EvolutionApiSettings>(
    builder.Configuration.GetSection("EvolutionApi"));

builder.Services.Configure<MessageSchedulerSettings>(
    builder.Configuration.GetSection(MessageSchedulerSettings.SectionName));

builder.Services.AddHttpClient<IEvolutionApiService, EvolutionApiService>();

if (builder.Configuration.GetValue<bool>("Scheduler:Enabled"))
    builder.Services.AddHostedService<MessageSchedulerService>();

// ==========================
// JWT AUTH
// ==========================

var jwtKey = builder.Configuration["Jwt:Key"]
             ?? throw new InvalidOperationException("JWT Key não configurada");

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
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
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
    
    // Configurar suporte para upload de arquivos (IFormFile)
    // Isso resolve o erro do Swagger ao gerar documentação para endpoints com IFormFile
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
    
    // Configurar suporte para List<IFormFile>
    c.MapType<List<IFormFile>>(() => new OpenApiSchema
    {
        Type = "array",
        Items = new OpenApiSchema
        {
            Type = "string",
            Format = "binary"
        }
    });
    
    // Configurar segurança JWT no Swagger
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

// ==========================
// CORS
// ==========================

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

// ==========================
// UPLOADS FIX DEFINITIVO
// ==========================

bool IsRunningOnAzure()
{
    return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));
}

string GetUploadsPath(IWebHostEnvironment env)
{
    if (IsRunningOnAzure())
    {
        var home = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrWhiteSpace(home))
            home = @"D:\home";

        return Path.Combine(home, "data", "uploads");
    }

    return Path.Combine(env.ContentRootPath, "uploads");
}

var uploadsPath = GetUploadsPath(app.Environment);

try
{
    Directory.CreateDirectory(uploadsPath);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Erro ao criar pasta de uploads em {UploadsPath}", uploadsPath);
}

app.Logger.LogInformation("UploadsPath definido como: {UploadsPath}", uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<SistemaIgreja.API.Permissions.PermissionMiddleware>();
app.MapControllers();

app.Run();