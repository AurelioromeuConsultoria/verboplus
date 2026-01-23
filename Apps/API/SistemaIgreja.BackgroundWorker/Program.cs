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
        services.AddDbContext<SistemaIgrejaDbContext>(opt =>
            opt.UseSqlServer(ctx.Configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPessoaRepository, PessoaRepository>();
        services.AddScoped<IPessoaPerfilRepository, PessoaPerfilRepository>();
        services.AddScoped<IVisitanteRepository, VisitanteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IConfiguracaoMensagemRepository, ConfiguracaoMensagemRepository>();
        services.AddScoped<IMensagemAgendadaRepository, MensagemAgendadaRepository>();
        services.AddScoped<IEquipeRepository, EquipeRepository>();
        services.AddScoped<ICargoRepository, CargoRepository>();
        services.AddScoped<IVoluntarioRepository, VoluntarioRepository>();

        services.AddScoped<IVisitanteService, VisitanteService>();
        services.AddScoped<IConfiguracaoMensagemService, ConfiguracaoMensagemService>();
        services.AddScoped<IMensagemAgendadaService, MensagemAgendadaService>();
        services.AddScoped<IEquipeService, EquipeService>();
        services.AddScoped<ICargoService, CargoService>();
        services.AddScoped<IVoluntarioService, VoluntarioService>();

        services.Configure<MessageSchedulerSettings>(
            ctx.Configuration.GetSection(MessageSchedulerSettings.SectionName));
        services.Configure<EvolutionApiSettings>(
            ctx.Configuration.GetSection("EvolutionApi"));

        services.AddHttpClient<IEvolutionApiService, EvolutionApiService>();

        services.AddHostedService<MessageSchedulerService>();
    })
    .ConfigureLogging(log => log.AddConsole())
    .Build();

await host.RunAsync();
