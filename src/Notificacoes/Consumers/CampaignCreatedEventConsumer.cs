using CS.Domain.Events;
using MassTransit;
using Notificacoes.Domain.Enums;
using Notificacoes.Domain.Shared.Interface;
using Notificacoes.Domain.Shared.Interfaces;

namespace Notificacoes.Consumers;
/// <summary>
/// Consumer responsável por processar eventos de pedidos realizados
/// </summary>
public class CampaignCreatedEventConsumer : IConsumer<CampaignCreatedEvent>
{
    private readonly IBaseLogger<CampaignCreatedEventConsumer> _logger;
    private readonly IEmailService _emailService;


    public CampaignCreatedEventConsumer(IBaseLogger<CampaignCreatedEventConsumer> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Envia e-mail quando um evento CampaignCreatedEvent é recebido
    /// </summary>
    public async Task Consume(ConsumeContext<CampaignCreatedEvent> context)
    {
        var campaignEvent = context.Message;

        _logger.LogInformation("Evento recebido: CampaignCreatedEvent", BaseLogType.EVENT, campaignEvent, campaignEvent.correlationId);


        try
        {

            _logger.LogInformation($"E-mail de campanha '{campaignEvent.nomeCampanha}' criada enviado para usuários.", BaseLogType.EVENT, campaignEvent, campaignEvent.correlationId);

        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao enviar e-mail de campanha criada com correlationId: {campaignEvent.correlationId}", BaseLogType.EVENT, ex,  campaignEvent, campaignEvent.correlationId);
            
            // Lançar exceção para que o MassTransit tente reprocessar a mensagem
            throw;
        }
    }
}
