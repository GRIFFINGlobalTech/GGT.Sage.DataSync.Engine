using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Griffin.DataSync.Service.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(
            IOptions<EmailOptions> options,
            ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendAsync( string recipients, string subject, string body, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(recipients))
            {
                throw new ArgumentException(
                    "Email recipients cannot be empty.",
                    nameof(recipients));
            }

            if (string.IsNullOrWhiteSpace(_options.SmtpServer))
            {
                throw new InvalidOperationException(
                    "SMTP server is not configured.");
            }

            if (string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                throw new InvalidOperationException(
                    "Email FromEmail is not configured.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            using var message = new MailMessage();

            message.From = new MailAddress(
                _options.FromEmail,
                string.IsNullOrWhiteSpace(_options.FromName)
                    ? _options.FromEmail
                    : _options.FromName);

            foreach (var recipient in recipients
                         .Split(
                             new[] { ';', ',' },
                             StringSplitOptions.RemoveEmptyEntries |
                             StringSplitOptions.TrimEntries))
            {
                message.To.Add(recipient);
            }

            message.Subject = subject;

            message.Body = body;

            message.IsBodyHtml = body.Contains("<html",
                StringComparison.OrdinalIgnoreCase);

            using var smtp = new SmtpClient(
                _options.SmtpServer,
                _options.SmtpPort);

            smtp.EnableSsl = _options.EnableSsl;

            smtp.Timeout =
                _options.TimeoutSeconds * 1000;

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                smtp.Credentials =
                    new NetworkCredential(
                        _options.Username,
                        _options.Password);
            }
            else
            {
                smtp.UseDefaultCredentials = true;
            }

            _logger.LogInformation(
                "Sending email. Subject: {Subject}, Recipients: {Recipients}",
                subject,
                recipients);

            await smtp.SendMailAsync(message);

            _logger.LogInformation(
                "Email sent successfully. Subject: {Subject}",
                subject);
        }
    }
}
