namespace Notificacoes.Domain.Shared.Interface
{
    public interface IEmailService
    {
        Task SendUserCreatedEmailAsync(string nome, string email, string cpf, CancellationToken ct = default);
        //Task SendCampaignCreatedEmailAsync(string nomeCampanha, string mensagemCampanha, DateTime dataInicio, DateTime dataTermino, CancellationToken ct = default);
        Task SendDonationProcessedEmailAsync(string nome, string email, string tituloCampanha, decimal valor, string status,CancellationToken ct = default);
    }
}
