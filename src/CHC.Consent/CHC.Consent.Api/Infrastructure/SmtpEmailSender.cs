using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace CHC.Consent.Api.Infrastructure
{
    public class SmtpEmailSender : IEmailSender
    {
        private SmtpEmailSenderOptions options;

        /// <inheritdoc />
        public SmtpEmailSender(IOptions<SmtpEmailSenderOptions> options)
        {
            this.options = options.Value;
        }

        /// <inheritdoc />
        public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
            new SmtpClient(options.Host, options.Port)
                .SendMailAsync(options.From, email, subject, htmlMessage);
    }

    public class SmtpEmailSenderOptions
    {
        public string From { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }
}