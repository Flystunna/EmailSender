using Shared.Core.Dtos;

namespace EmailService.Interfaces
{
    public interface IEmailLogService
    {
        Task<bool> OnboardUser(OnboardUserDto model);
        Task<List<EmailLogDto>> GetAllLogs(FilterDto input);
        Task<Guid?> Resend(ResendEmailDto id);
    }
}
