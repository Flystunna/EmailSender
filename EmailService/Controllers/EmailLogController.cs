using EmailService.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Core.Dtos;

namespace EmailService.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class EmailLogController : ControllerBase
    {
        private readonly IEmailLogService _emailLogService;
        public EmailLogController(IEmailLogService emailLogService)
        {
            _emailLogService = emailLogService;
        }
        /// <summary>
        /// Get all email logs. Passcodes/Passwords are encrypted
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("GetAllLogs")]
        [HttpPost]
        public async Task<IActionResult> GetAllLogs(FilterDto model)
        {
            try
            {
                return Ok(await _emailLogService.GetAllLogs(model));
            }
            catch (Exception e)
            {
                return BadRequest($"An error must have occurred {e.Message}");
            }
        }
        /// <summary>
        /// Onboard user method to send email for onboarding new users
        /// Passwords/Passcodes are generated using cryptography and encrypted when saving logs but decrypted when sending to user email
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("onboarduser")]
        [HttpPost]
        public async Task<IActionResult> OnboardUser(OnboardUserDto model)
        {
            try
            {
                await _emailLogService.OnboardUser(model);
                return Ok(true);
            }
            catch (Exception e)
            {
                return BadRequest($"An error must have occurred {e.Message}");
            }
        }
        /// <summary>
        /// Resend a particular email log using the Guid
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Route("Resend")]
        [HttpPost]
        public async Task<IActionResult> Resend(ResendEmailDto model)
        {
            try
            {
                var resend = await _emailLogService.Resend(model);           
                if (resend == null)
                    return NotFound();                    
                return Ok(resend);
            }
            catch (Exception e)
            {
                return BadRequest($"An error must have occurred {e.Message}");
            }
        }
    }
}
