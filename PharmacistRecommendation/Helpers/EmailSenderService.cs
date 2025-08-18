using MimeKit;
using MailKit.Net.Smtp;

namespace PharmacistRecommendation.Helpers
{
    public class EmailSenderService
    {
        private readonly EmailConfiguration _config;

        public EmailSenderService(EmailConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Farmacia", _config.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { TextBody = body };
            if (File.Exists(attachmentPath))
                builder.Attachments.Add(attachmentPath);

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config.SenderEmail, _config.SenderAppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
