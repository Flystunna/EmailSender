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
