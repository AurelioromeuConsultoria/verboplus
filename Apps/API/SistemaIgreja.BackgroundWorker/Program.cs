using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaIgreja.Application.Configuration;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;
using SistemaIgreja.Infrastructure.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // ==========================
        // DATABASE CONFIGURATION
        // ==========================

        var databaseProvider = ctx.Configuration["Database:Provider"] ?? "SqlServer";
        var connectionString = ctx.Configuration.GetConnectionString("DefaultConnection");

        if (databaseProvider.Equals("postgresql", StringComparison.OrdinalIgnoreCase) ||
            databaseProvider.Equals("postgres", StringComparison.OrdinalIgnoreCase))
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }

        services.AddDbContext<SistemaIgrejaDbContext>(options =>
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

        services.AddScoped<IPessoaRepository, PessoaRepository>();
        services.AddScoped<IPessoaPerfilRepository, PessoaPerfilRepository>();
        services.AddScoped<IVisitanteRepository, VisitanteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IConfiguracaoMensagemRepository, ConfiguracaoMensagemRepository>();
        services.AddScoped<IMensagemAgendadaRepository, MensagemAgendadaRepository>();
        services.AddScoped<IEquipeRepository, EquipeRepository>();
        services.AddScoped<ICargoRepository, CargoRepository>();
        services.AddScoped<IVoluntarioRepository, VoluntarioRepository>();
        services.AddScoped<IEventoRepository, EventoRepository>();
        services.AddScoped<IEventoOcorrenciaRepository, EventoOcorrenciaRepository>();
        services.AddScoped<IEscalaRepository, EscalaRepository>();
        services.AddScoped<IConfiguracaoCampanhaAniversarioRepository, ConfiguracaoCampanhaAniversarioRepository>();
        services.AddScoped<IEnvioCampanhaAniversarioRepository, EnvioCampanhaAniversarioRepository>();

        services.AddScoped<IVisitanteService, VisitanteService>();
        services.AddScoped<IConfiguracaoMensagemService, ConfiguracaoMensagemService>();
        services.AddScoped<IMensagemAgendadaService, MensagemAgendadaService>();
        services.AddScoped<IEquipeService, EquipeService>();
        services.AddScoped<ICargoService, CargoService>();
        services.AddScoped<IVoluntarioService, VoluntarioService>();
        services.AddScoped<IEventoOcorrenciaService, EventoOcorrenciaService>();
        services.AddScoped<IEscalaService, EscalaService>();
        services.AddScoped<ICampanhaAniversarioService, CampanhaAniversarioService>();
        services.AddSingleton<ISchedulerExecutionMonitor, SchedulerExecutionMonitor>();

        services.Configure<MessageSchedulerSettings>(
            ctx.Configuration.GetSection(MessageSchedulerSettings.SectionName));
        services.Configure<EscalaSchedulerSettings>(
            ctx.Configuration.GetSection(EscalaSchedulerSettings.SectionName));
        services.Configure<BirthdayCampaignSchedulerSettings>(
            ctx.Configuration.GetSection(BirthdayCampaignSchedulerSettings.SectionName));
        services.Configure<EvolutionApiSettings>(
            ctx.Configuration.GetSection("EvolutionApi"));
        services.Configure<PublicAppUrlSettings>(
            ctx.Configuration.GetSection(PublicAppUrlSettings.SectionName));

        services.AddHttpClient<IEvolutionApiService, EvolutionApiService>();

        services.AddHostedService<MessageSchedulerService>();
        services.AddHostedService<EscalaSchedulerService>();
        services.AddHostedService<BirthdayCampaignSchedulerService>();
    })
    .ConfigureLogging(log => log.AddConsole())
    .Build();

await host.RunAsync();
