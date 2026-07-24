using CS.Domain.Events;
using MassTransit;
using Notificacoes.Domain.Enums;
using Notificacoes.Domain.Shared.Interface;
using Notificacoes.Domain.Shared.Interfaces;

namespace Notificacoes.Consumers;
/// <summary>
/// Consumer responsável por processar eventos de pedidos realizados
/// </summary>
public class DonationProcessedEventConsumer : IConsumer<DonationProcessedEvent>
{
    private readonly IBaseLogger<DonationProcessedEventConsumer> _logger;
    private readonly IEmailService _emailService;


    public DonationProcessedEventConsumer(IBaseLogger<DonationProcessedEventConsumer> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Envia e-mail quando um evento DonationProcessedEvent é recebido
    /// </summary>
    public async Task Consume(ConsumeContext<DonationProcessedEvent> context)
    {
        var donationEvent = context.Message;

        _logger.LogInformation("Evento recebido: DonationProcessedEvent", BaseLogType.EVENT, donationEvent, donationEvent.correlationId);


        try
        {
            await _emailService.SendDonationProcessedEmailAsync(donationEvent.nome, donationEvent.email, donationEvent.tituloCampanha, donationEvent.valor, donationEvent.status);

            _logger.LogInformation($"E-mail de doação processada enviado para {donationEvent.email}.", BaseLogType.EVENT, donationEvent,donationEvent.correlationId);

        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao enviar e-mail de doação processada para o usuário: {donationEvent.email}", BaseLogType.EVENT, ex, 
                donationEvent, donationEvent.correlationId);
            
            // Lançar exceção para que o MassTransit tente reprocessar a mensagem
            throw;
        }
    }
}
