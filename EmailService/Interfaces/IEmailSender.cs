using EmailService.Services;

namespace EmailService.Interfaces
{
    public interface IEmailSender
    {
        Task<bool> SendMailAsync(MailBase mail, Dictionary<string, string> replacements);
    }
}
