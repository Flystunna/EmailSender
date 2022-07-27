using Shared.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Entities
{
    [Table("EmailLogs")]
    public class EmailLog
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        [StringLength(255)]
        public string To { get; set; }

        [Required]
        [StringLength(255)]
        public string Subject { get; set; }        

        [StringLength(4000)]
        public string Body { get; set; }

        [StringLength(255)]
        public string Response { get; set; }
        
        public AppSource AppSource { get; set; }
        public EmailType EmailType { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.Now;

        public EmailLog()
        {
            
        }
        public EmailLog(Guid id, string to, string subject, string body, string response, AppSource appSource, EmailType emailType)
        {
            Id = id;
            To = to;
            Subject = subject;
            Body = body;
            Response = response;
            AppSource = appSource;
            EmailType = emailType;
        }
    }
}
