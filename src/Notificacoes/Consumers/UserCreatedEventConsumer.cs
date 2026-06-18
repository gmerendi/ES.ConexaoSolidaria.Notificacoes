using CS.Domain.Events;
using MassTransit;
using Notificacoes.Domain.Enums;
using Notificacoes.Domain.Shared.Interface;
using Notificacoes.Domain.Shared.Interfaces;

namespace Notificacoes.Consumers;
/// <summary>
/// Consumer responsável por processar eventos de pedidos realizados
/// </summary>
public class UserCreatedEventConsumer : IConsumer<UserCreatedEvent>
{
    private readonly IBaseLogger<UserCreatedEventConsumer> _logger;
    private readonly IEmailService _emailService;


    public UserCreatedEventConsumer(IBaseLogger<UserCreatedEventConsumer> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Envia e-mail quando um evento UserCreatedEvent é recebido
    /// </summary>
    public async Task Consume(ConsumeContext<UserCreatedEvent> context)
    {
        var userEvent = context.Message;

        _logger.LogInformation("Evento recebido: UserCreatedEvent", BaseLogType.EVENT, userEvent, userEvent.correlationId);


        try
        {
            await _emailService.SendUserCreatedEmailAsync(userEvent.nomeCompleto, userEvent.email, userEvent.cpf);
           
            _logger.LogInformation("E-mail de usuario criado enviado para " + userEvent.email, BaseLogType.EVENT, userEvent, userEvent.correlationId);

        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao enviar e-mail de usuario criado com correlationId: " + userEvent.correlationId,
                BaseLogType.EVENT, ex, userEvent.correlationId);
            
            // Lançar exceção para que o MassTransit tente reprocessar a mensagem
            throw;
        }
    }
}
