using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SistemaIgreja.Application.Interfaces;
using SistemaIgreja.Application.Services;
using SistemaIgreja.Infrastructure.Data;
using SistemaIgreja.Infrastructure.Repositories;
using SistemaIgreja.Infrastructure.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // DbContext
        services.AddDbContext<SistemaIgrejaDbContext>(opt =>
          opt.UseSqlServer(ctx.Configuration.GetConnectionString("DefaultConnection")));

        // Repositórios
        services.AddScoped<IVisitanteRepository, VisitanteRepository>();
        services.AddScoped<IConfiguracaoMensagemRepository, ConfiguracaoMensagemRepository>();
        services.AddScoped<IMensagemAgendadaRepository, MensagemAgendadaRepository>();
        // Novos repositórios
        services.AddScoped<IEquipeRepository, EquipeRepository>();
        services.AddScoped<ICargoRepository, CargoRepository>();
        services.AddScoped<IVoluntarioRepository, VoluntarioRepository>();

        // Serviços
        services.AddScoped<IVisitanteService, VisitanteService>();
        services.AddScoped<IConfiguracaoMensagemService, ConfiguracaoMensagemService>();
        services.AddScoped<IMensagemAgendadaService, MensagemAgendadaService>();
        // Novos serviços
        services.AddScoped<IEquipeService, EquipeService>();
        services.AddScoped<ICargoService, CargoService>();
        services.AddScoped<IVoluntarioService, VoluntarioService>();

        // Scheduler
        services.AddHostedService<MessageSchedulerService>();
    })
    .ConfigureLogging(log => log.AddConsole())
    .Build();

using (var scope = host.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<SistemaIgrejaDbContext>();
    context.Database.Migrate();
}


await host.RunAsync();
