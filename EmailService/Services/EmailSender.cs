using EmailService.Interfaces;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;

namespace EmailService.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly StringComparison _stringComparison = StringComparison.OrdinalIgnoreCase;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }
        private SmtpClient GetSmtpClient()
        {
            var client = new SmtpClient
            {
                Host = _config["SMTP:Host"],
                Port = int.Parse(_config["SMTP:Port"]),
                EnableSsl = Convert.ToBoolean(_config["SMTP:EnableSsl"]),
                UseDefaultCredentials = Convert.ToBoolean(_config["SMTP:UseDefaultCredentials"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                TargetName = _config["SMTP:TargetName"]
            };

            if (!client.UseDefaultCredentials)
                client.Credentials = new NetworkCredential(_config["SMTP:Username"], _config["SMTP:Password"]);
            return client;
        }

        public async Task<bool> SendMailAsync(MailBase mail, Dictionary<string, string> replacements)
        {
            var message = await BuildMailMessage(mail, replacements);
            try
            {
                using (var _smtpClient = GetSmtpClient())
                {
                    await _smtpClient.SendMailAsync(message);
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        protected virtual void SendMail(MailBase mail, Dictionary<string, string> replacements)
        {
            var message = BuildMailMessage(mail, replacements).Result;
            try
            {
                using (var _smtpClient = GetSmtpClient())
                {
                    _smtpClient.Send(message);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        void ValidateMail(MailBase mail)
        {
            if (mail.To == null || !mail.To.Any())
                throw new ArgumentNullException("To");

            if (string.IsNullOrWhiteSpace(mail.Sender))
                throw new ArgumentNullException("Sender");

            if (string.IsNullOrWhiteSpace(mail.Subject))
                throw new ArgumentNullException("Subject");

            if (!mail.BodyIsFile && string.IsNullOrWhiteSpace(mail.Body))
                throw new ArgumentNullException("Body");

            if (mail.BodyIsFile && string.IsNullOrWhiteSpace(mail.BodyPath))
                throw new ArgumentNullException("BodyPath");
        }
        private Task<string> GetEmailBodyTemplate(string templateLocation)
        {
            return ReadTemplateFileContent(templateLocation);
        }
        private async Task<string> ReadTemplateFileContent(string templateLocation)
        {
            StreamReader sr;
            string body;
            try
            {
                if (templateLocation.ToLower().StartsWith("http"))
                {
                    var wc = new HttpClient();
                    sr = new StreamReader(await wc.GetStreamAsync(templateLocation));
                }
                else
                    sr = new StreamReader(templateLocation, Encoding.Default);
                body = sr.ReadToEnd();
                sr.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return body;
        }
        private async Task<MailMessage> BuildMailMessage(MailBase mail, Dictionary<string, string> replacements)
        {
            ValidateMail(mail);

            var sender = new MailAddress(mail.Sender, mail.SenderDisplayName);

            var mailMessage = new MailMessage()
            {
                Subject = mail.Subject,
                IsBodyHtml = mail.IsBodyHtml,
                From = sender,
            };

            var mailBody = !mail.BodyIsFile ? mail.Body : await GetEmailBodyTemplate(mail.BodyPath);

            if (replacements != null)
                mailBody = Replace(mailBody, replacements, false);

            mailMessage.Body = mailBody;

            if (mail.Attachments != null && mail.Attachments.Any())
                foreach (var attachment in mail.Attachments)
                    mailMessage.Attachments.Add(attachment);

            foreach (var to in mail.To)
                mailMessage.To.Add(to);

            if (mail.Bcc != null && mail.Bcc.Any())
                foreach (var bcc in mail.Bcc)
                    mailMessage.Bcc.Add(bcc);

            if (mail.CC != null && mail.CC.Any())
                foreach (var cc in mail.CC)
                    mailMessage.CC.Add(cc);

            return mailMessage;
        }
        public string Replace(string template, Dictionary<string, string> tokens, bool htmlEncode)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new ArgumentNullException("template");

            if (tokens == null)
                throw new ArgumentNullException("tokens");

            foreach (string key in tokens.Keys)
            {
                string? tokenValue = tokens[key];
                //do not encode URLs
                if (htmlEncode)
                    tokenValue = HtmlEncoder.Default.Encode(tokenValue);
                var replaceable = "{{" + key + "}}";
                template = Replace(template, replaceable, tokenValue);
            }
            return template;
        }
        private string Replace(string original, string pattern, string replacement)
        {
            if (_stringComparison == StringComparison.Ordinal)
            {
                return original.Replace(pattern, replacement);
            }
            else
            {
                int count, position0, position1;
                count = position0 = position1 = 0;
                int inc = (original.Length / pattern.Length) * (replacement.Length - pattern.Length);
                char[] chars = new char[original.Length + Math.Max(0, inc)];
                while ((position1 = original.IndexOf(pattern, position0, _stringComparison)) != -1)
                {
                    for (int i = position0; i < position1; ++i)
                        chars[count++] = original[i];
                    for (int i = 0; i < replacement.Length; ++i)
                        chars[count++] = replacement[i];
                    position0 = position1 + pattern.Length;
                }
                if (position0 == 0) return original;
                for (int i = position0; i < original.Length; ++i)
                    chars[count++] = original[i];
                return new string(chars, 0, count);
            }
        }
    }




    //Todo: attachment should be part of the Mail
    public abstract class MailBase
    {
        public bool BodyIsFile { get; set; }
        public string Body { get; set; }
        public string BodyPath { get; set; }
        public string Subject { get; set; }
        public string Sender { get; set; }
        public string SenderDisplayName { get; set; }
        public bool IsBodyHtml { get; set; }
        public ICollection<string> To { get; set; }
        public ICollection<string> Bcc { get; set; }
        public ICollection<string> CC { get; set; }
        public ICollection<Attachment> Attachments { get; set; }
    }

    public sealed class Mail : MailBase
    {
        private Mail()
        {
            IsBodyHtml = true;
            To = new List<string>();
            CC = new List<string>();
            Bcc = new List<string>();
            Attachments = new List<Attachment>();
        }

        public Mail(string sender, string subject, params string[] to)
            : this()
        {
            Sender = sender;
            Subject = subject;

            foreach (var rec in to)
                To.Add(rec);
        }
    }
}
