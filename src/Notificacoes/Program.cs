using Notificacoes;
using Notificacoes.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);
var logCounter = 0;
var logTotal = 3;

// ──────────────────────────────────────────────────────────────────────────────
// ── Configuration
// ──────────────────────────────────────────────────────────────────────────────
ConfigureAppSettings(builder.Configuration);


// ──────────────────────────────────────────────────────────────────────────────
// ── Logs
// ──────────────────────────────────────────────────────────────────────────────
using var loggerFactory = LoggerFactory.Create(logging => {
    logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    logging.AddSimpleConsole();
});
var logger = loggerFactory.CreateLogger("Program");
logger.LogInformation(" ***** ({0}/{1}) - Inicializando Notificacoes ", logCounter++, logTotal);


// ──────────────────────────────────────────────────────────────────────────────
// ── Infrastructure Extensions
// ──────────────────────────────────────────────────────────────────────────────
logger.LogInformation(" ***** ({0}/{1}) - Inicio inicialização de Infrastructure Extensions ", logCounter++, logTotal);
builder.Services.AddCustomLogging(logger);
builder.Services.AddMessaging(builder.Configuration,logger);
builder.Services.AddAuditLog(builder.Configuration, logger);
builder.Services.AddEmail(logger);
logger.LogInformation(" ***** ({0}/{1}) - Termino inicialização de Infrastructure Extensions ", logCounter, logTotal);


builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();




#region Helper Methods
void ConfigureAppSettings(ConfigurationManager config)
{
    string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                       ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                       ?? "Production";

    config.SetBasePath(Directory.GetCurrentDirectory())
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
          .AddEnvironmentVariables();
}
#endregion

