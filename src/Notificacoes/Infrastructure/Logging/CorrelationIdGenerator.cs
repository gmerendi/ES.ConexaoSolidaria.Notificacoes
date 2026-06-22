using Notificacoes.Domain.Shared.Interfaces;

namespace Notificacoes.Infrastructure.Services.Logging
{
    public class CorrelationIdGenerator : ICorrelationIdGenerator
    {
        private static string _correlationId;

        public string Get() => _correlationId;

        public void Set(string correlationId) => _correlationId = correlationId;
    }
}
