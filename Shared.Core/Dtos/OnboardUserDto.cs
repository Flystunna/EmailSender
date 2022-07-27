using Shared.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Dtos
{
    public class OnboardUserDto
    {
        [StringLength(255, ErrorMessage = "Full name must be less than 255 characters")]
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Phone number is not valid")]
        [StringLength(12, ErrorMessage = "Phone number must be less than 12 characters")]
        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email must be valid")]
        [StringLength(255, ErrorMessage = "Email must be less than 255 characters")]
        [Required(ErrorMessage = "Email address is required")]
        public string Email { get; set; }

        [StringLength(255, ErrorMessage = "Username must be less than 255 characters")]
        public string Username { get; set; }
        public AppSource AppSource { get; set; }

        public OnboardUserDto(string fullName, string phoneNumber, string username, string email, AppSource appSource)
        {
            FullName = fullName;
            PhoneNumber = phoneNumber;
            Email = email;
            Username = username;
            AppSource = appSource;
        }
    }
}
