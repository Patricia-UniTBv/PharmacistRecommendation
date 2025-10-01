using MimeKit;
using MailKit.Net.Smtp;

namespace PharmacistRecommendation.Helpers
{
    public class EmailSenderService
    {
        private readonly EmailConfiguration _config;
        private int pharmacyId { get; set; }

        public EmailSenderService(EmailConfiguration config)
        {
            _config = config;
            pharmacyId = SessionManager.GetCurrentPharmacyId() ?? 1;
        }

        public async Task SendEmailWithAttachmentAsync(string pharmaName, string toEmail, string subject, string body, string attachmentPath)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(pharmaName, _config.SenderEmail));
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

        public async Task SendEmailWithAttachmentsAsync(
        string pharmaName,
     string toEmail,
     string subject,
     string body,
     List<(string FileName, string FilePath)> attachmentsFiles)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(pharmaName, _config.SenderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { TextBody = body };

            if (attachmentsFiles != null)
            {
                foreach (var (fileName, filePath) in attachmentsFiles)
                {
                    if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
                    {
                        builder.Attachments.Add(fileName, File.ReadAllBytes(filePath));
                    }
                }
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_config.SenderEmail, _config.SenderAppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }


    }
}
