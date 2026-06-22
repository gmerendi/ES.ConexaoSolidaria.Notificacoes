using Notificacoes.Domain.Shared.Interfaces;

namespace Notificacoes
{
    public class Worker : BackgroundService
    {
        private readonly IBaseLogger<Worker> _logger;
        private const string HealthFilePath = "/tmp/healthy";

        public Worker(IBaseLogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Loop principal do Worker (respeita o ciclo de vida do Kubernetes via stoppingToken)
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Atualiza o arquivo de saúde do Kubernetes
                    await File.WriteAllTextAsync(HealthFilePath, DateTime.UtcNow.ToString(), stoppingToken);

                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao atualizar o arquivo de Health Check.", Domain.Enums.BaseLogType.LOG, ex.Message);
                }

                // Aguarda 10 segundos antes do próximo batimento cardíaco (Heartbeat)
                // Se o K8s pedir para o Pod morrer, o stoppingToken cancela o Delay na hora!
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}