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
using SistemaIgreja.Application.Configuration;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database (EF Core)
// =======================
var databaseProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Npgsql legacy timestamp behavior (se você usa timestamps sem timezone)
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

// =======================
// Repositórios
// =======================
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

// =======================
// Serviços
// =======================
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

// Kids
builder.Services.AddScoped<IKidsService, KidsService>();

// =======================
// Settings / HttpClient / Scheduler
// =======================
builder.Services.Configure<EvolutionApiSettings>(builder.Configuration.GetSection("EvolutionApi"));
builder.Services.Configure<MessageSchedulerSettings>(builder.Configuration.GetSection(MessageSchedulerSettings.SectionName));

builder.Services.AddHttpClient<IEvolutionApiService, EvolutionApiService>();

var schedulerEnabled = builder.Configuration.GetValue<bool>("Scheduler:Enabled");
if (schedulerEnabled)
{
    builder.Services.AddHostedService<MessageSchedulerService>();
}

// =======================
// JWT Auth
// =======================
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

// =======================
// Controllers + Swagger
// =======================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sistema Igreja API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Ex: \"Authorization: Bearer {token}\"",
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

// =======================
// CORS
// =======================
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

// =======================
// Pipeline
// =======================
app.UseCors();

// Swagger: mantenho só em Development, como você já tinha
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // você tinha removido, mantive removido

app.UseStaticFiles(); // wwwroot (somente leitura no App Service quando Run-From-Package)

// =======================
// UPLOADS (CORRIGIDO PARA APP SERVICE)
// - NUNCA grave em wwwroot em produção no App Service.
// - Use D:\home\data\uploads (persistente)
// =======================
string uploadsPath;

if (app.Environment.IsDevelopment())
{
    // Local: dentro do projeto
    uploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
}
else
{
    // App Service Windows: HOME normalmente é D:\home (ou variável HOME)
    var home = Environment.GetEnvironmentVariable("HOME");
    if (string.IsNullOrWhiteSpace(home))
        home = @"D:\home";

    uploadsPath = Path.Combine(home, "data", "uploads");
}

Directory.CreateDirectory(uploadsPath);

// log útil pra você confirmar no Log Stream qual path está rodando
app.Logger.LogInformation("UPLOADS PATH = {UploadsPath}", uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<SistemaIgreja.API.Permissions.PermissionMiddleware>();

app.MapControllers();

// =======================
// Migrations automáticas
// (recomendado: NÃO rodar em produção automaticamente)
// =======================
var runMigrations = app.Configuration.GetValue<bool>("Database:RunMigrations");

if (app.Environment.IsDevelopment() && runMigrations)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SistemaIgrejaDbContext>();

    try
    {
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Falha ao executar migrations. API iniciando mesmo assim.");
    }
}

app.Run();