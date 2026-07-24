using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Notificacoes.Domain.Enums;
using Notificacoes.Domain.Shared.Interface;
using Notificacoes.Domain.Shared.Interfaces;

namespace Notificacoes.Infrastructure.Services.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IBaseLogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, IBaseLogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendUserCreatedEmailAsync(
        string nome,
        string email,      
        string cpf,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio.", nameof(email));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));

        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _configuration["Email:NomeRemetente"] ?? "Conexão Solidária",
                _configuration["Email:EmailRemetente"] ?? "noreply@conexaosolidaria.com.br"));

            message.To.Add(new MailboxAddress(nome, email));
            message.Subject = "Conexão Solidária - Usuário criado";
            message.Body = new TextPart("html")
            {
                Text = GerarEmailUsuarioCriado(nome, cpf)
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["Email:SmtpHost"] ?? "mailpit",
                int.Parse(_configuration["Email:SmtpPort"] ?? "1025"),
                SecureSocketOptions.None,  // Mailpit não usa TLS
                ct);

            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private static string GerarEmailUsuarioCriado(string nome, string cpf) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <div style="background-color: #4CAF50; padding: 20px; border-radius: 8px 8px 0 0;">
                <h1 style="color: white; margin: 0;">💚 Conexão Solidária</h1>
            </div>
            <div style="background-color: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px;">
                <h2>Olá, {nome}!</h2>
                <p>Seu usuário foi criado em nosso sistema com o cpf: {cpf}!</p>
                
                <p>Verifique as campanhas ativas em nosso site <a href="https://www.conexaosolidaria.com.br">www.conexaosolidaria.com.br</a></p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="color: #999; font-size: 12px;">
                    Este é um email automático. Por favor, não responda.
                </p>
            </div>
        </body>
        </html>
        """;


    public async Task SendDonationProcessedEmailAsync(
        string nome,
        string email,
        string tituloCampanha,
        decimal valor,
        string status,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio.", nameof(email));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome não pode ser vazio.", nameof(nome));

        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                _configuration["Email:NomeRemetente"] ?? "Conexão Solidária",
                _configuration["Email:EmailRemetente"] ?? "noreply@conexaosolidaria.com.br"));

            message.To.Add(new MailboxAddress(nome, email));
            message.Subject = (status == DoacaoStatus.APROVADA.ToString()) ? "Conexão Solidária - Doacao Processada" : "Conexão Solidária - Doacao Recusada";
            message.Body = new TextPart("html")
            {
                Text = (status == DoacaoStatus.APROVADA.ToString()) ? GerarEmailDoacaoAprovada(nome, tituloCampanha, valor) : GerarEmailDoacaoRecusada(nome, tituloCampanha, valor)
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["Email:SmtpHost"] ?? "mailpit",
                int.Parse(_configuration["Email:SmtpPort"] ?? "1025"),
                SecureSocketOptions.None,  // Mailpit não usa TLS
                ct);

            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            throw;
        }
    }


    private static string GerarEmailDoacaoAprovada(string nome, string tituloCampanha, decimal valor) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <div style="background-color: #4CAF50; padding: 20px; border-radius: 8px 8px 0 0;">
                <h1 style="color: white; margin: 0;">💚 Conexão Solidária</h1>
            </div>
            <div style="background-color: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px;">
                <h2>Olá, {nome}!</h2>
                <p>Sua doação foi processada com sucesso! Obrigado por fazer a diferença</p>
                <div style="background-color: white; padding: 20px; border-radius: 8px; border-left: 4px solid #4CAF50; margin: 20px 0;">
                    <p><strong>Campanha:</strong> {tituloCampanha}</p>
                    <p><strong>Valor doado:</strong> R$ {valor:F2}</p>
                    <p><strong>Data:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                </div>
                <p>Sua generosidade ajuda a transformar vidas. Muito obrigado! 🙏</p>
                
                <p>Verifique as campanhas ativas em nosso site <a href="https://www.conexaosolidaria.com.br">www.conexaosolidaria.com.br</a></p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="color: #999; font-size: 12px;">
                    Este é um email automático. Por favor, não responda.
                </p>
            </div>
        </body>
        </html>
        """;


    private static string GerarEmailDoacaoRecusada(string nome, string tituloCampanha, decimal valor) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
            <div style="background-color: #4CAF50; padding: 20px; border-radius: 8px 8px 0 0;">
                <h1 style="color: white; margin: 0;">💚 Conexão Solidária</h1>
            </div>
            <div style="background-color: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px;">
                <h2>Olá, {nome}!</h2>
                <p>Infelizmente sua doação foi recusada! Por favor, verifique os dados do pagamento ou tente novamente com outra forma de pagamento.</p>
                <div style="background-color: white; padding: 20px; border-radius: 8px; border-left: 4px solid #4CAF50; margin: 20px 0;">
                    <p><strong>Campanha:</strong> {tituloCampanha}</p>
                    <p><strong>Valor doado:</strong> R$ {valor:F2}</p>
                    <p><strong>Data:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                </div>
                <p>Ainda dá tempo de transformar vidas! 🙏</p>
                
                <p>Verifique as campanhas ativas em nosso site <a href="https://www.conexaosolidaria.com.br">www.conexaosolidaria.com.br</a></p>
                <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
                <p style="color: #999; font-size: 12px;">
                    Este é um email automático. Por favor, não responda.
                </p>
            </div>
        </body>
        </html>
        """;
}