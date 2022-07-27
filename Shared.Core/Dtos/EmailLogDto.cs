using Shared.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Core.Dtos
{
    public class EmailLogDto
    {
        public Guid Id { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string Response { get; set; }

        public AppSource AppSource { get; set; }
        public EmailType EmailType { get; set; }
        public DateTime CreationTime { get; set; }
    }
}
