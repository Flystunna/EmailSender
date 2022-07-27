using EmailService.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Dtos;
using Shared.Core.Entities;
using Shared.Core.Enums;
using Shared.Core.Helpers;
using Shared.EntityFrameworkCore;

namespace EmailService.Services
{
    public class EmailLogService: IEmailLogService
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _config;
        public readonly IEmailSender _emailSender;
        public EmailLogService(IHostEnvironment hostingEnvironment, IEmailSender emailSender, ApplicationDbContext dbContext, IConfiguration config)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _emailSender = emailSender;
            _config = config;
        }
        /// <summary>
        /// Resend a particular email log using the Guid
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<Guid?> Resend(ResendEmailDto input)
        {
            var emailLog = await _dbContext.EmailLog.FirstOrDefaultAsync(x => x.Id == input.Id);

            if (emailLog == null)
                return null;
            
            var replacement = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(emailLog.Body);
            if (replacement == null)
                throw new Exception("Invalid Email Body");
            
            DecryptPass(emailLog.AppSource, replacement);
            
            var bodyPath = GetEmailPath(emailLog.AppSource);

            var mail = new Mail(_config["SMTP:Username"], "Bankly: New Account information", emailLog.To)
            {
                BodyIsFile = true,
                BodyPath = bodyPath,
                SenderDisplayName = _config["SMTP:HostDisplayName"]
            };
            await _emailSender.SendMailAsync(mail, replacement);
            await LogResponse(input.Id, "Success");
            return input.Id;
        }
        /// <summary>
        /// Get all email logs. Passcodes/Passwords are encrypted
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<List<EmailLogDto>> GetAllLogs(FilterDto input)
        {
            var logs = await _dbContext.EmailLog
                .WhereIf(!string.IsNullOrEmpty(input.Filter), u => u.To.ToLower().Contains(input.Filter.ToLower())
                || u.Subject.ToLower().Contains(input.Filter.ToLower())
                || u.Response.ToLower().Contains(input.Filter.ToLower())
                || u.Id.ToString() == input.Filter.ToLower())
                .ToListAsync();

            return logs.Select(x => new EmailLogDto
            {
                Subject = x.Subject,
                AppSource = x.AppSource,
                Body = x.Body,
                CreationTime = x.CreationTime,
                EmailType = x.EmailType,
                Id = x.Id,
                Response = x.Response,
                To = x.To
            }).ToList();
        }
        /// <summary>
        /// Onboard user method to send email for onboarding new users
        /// Passwords/Passcodes are generated using cryptography and encrypted when saving logs but decrypted when sending to user email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> OnboardUser(OnboardUserDto model)
        {
            var id = Guid.NewGuid();
            var pass = Helpers.GeneratePass(model.AppSource);
            try
            {
                var replacement = new Dictionary<string, string>();
                string subject = "Bankly: New Account information";              
                string bodyPath = GetEmailPath(model.AppSource);
                string? body;
                if (model.AppSource == AppSource.Mobile)
                {
                    replacement = new Dictionary<string, string>
                    {
                        ["Passcode"] = EncryptionHelper.Encrypt(pass, _config["EncryptionKey"]),
                        ["EmailAddress"] = model.Email,
                        ["FullName"] = model.FullName,
                        ["PhoneNumber"] = model.PhoneNumber,
                        ["CreationTime"] = DateTime.Now.ToString("f")
                    };
                    body = System.Text.Json.JsonSerializer.Serialize(replacement);
                }
                else
                {
                    replacement = new Dictionary<string, string>
                    {
                        ["Username"] = model.Username,
                        ["Password"] = EncryptionHelper.Encrypt(pass, _config["EncryptionKey"]),
                        ["EmailAddress"] = model.Email,
                        ["FullName"] = model.FullName,
                        ["PhoneNumber"] = model.PhoneNumber,
                        ["CreationTime"] = DateTime.Now.ToString("f")
                    };
                    body = System.Text.Json.JsonSerializer.Serialize(replacement);
                }
                               
                await _dbContext.EmailLog.AddAsync(new EmailLog
                {
                    Id = id,
                    To = model.Email,
                    Subject = subject,
                    Body = body,
                    AppSource = model.AppSource,
                    EmailType = EmailType.NewUser
                });
                await _dbContext.SaveChangesAsync();

                DecryptPass(model.AppSource, replacement);
                
                var mail = new Mail(_config["SMTP:Username"], "Bankly: New Account information", model.Email)
                {
                    BodyIsFile = true,
                    BodyPath = bodyPath,
                    SenderDisplayName = _config["SMTP:HostDisplayName"]
                };
                await _emailSender.SendMailAsync(mail, replacement);
                await LogResponse(id, "Success");
                return true;
            }
            catch (Exception ex)
            {
                await LogResponse(id, $"An error occurred: {ex.Message}");
                throw;
            }
        }
        /// <summary>
        /// Log response for email logs
        /// </summary>
        /// <param name="emailId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task LogResponse(Guid emailId, string message)
        {
            var emailLog = await _dbContext.EmailLog.FirstOrDefaultAsync(x => x.Id == emailId);
            if(emailLog != null)
            {
                emailLog.Response = message;
                await _dbContext.SaveChangesAsync();
            }
        }
        /// <summary>
        /// Get email template path based off app source
        /// </summary>
        /// <param name="appSource"></param>
        /// <returns></returns>
        private string GetEmailPath(AppSource appSource)
        {
            if (appSource == AppSource.Mobile)
                return Path.Combine(_hostingEnvironment.ContentRootPath, "MessagingTemplates/new-account-mobile.html");
            else
                return Path.Combine(_hostingEnvironment.ContentRootPath, "MessagingTemplates/new-account-web.html");
        }
        private void DecryptPass(AppSource appSource, Dictionary<string, string> replacement)
        {
            if (appSource == AppSource.Mobile)
                replacement["Passcode"] = EncryptionHelper.Decrypt(replacement["Passcode"], _config["EncryptionKey"]);
            else
                replacement["Password"] = EncryptionHelper.Decrypt(replacement["Password"], _config["EncryptionKey"]);
        }
    }
}
