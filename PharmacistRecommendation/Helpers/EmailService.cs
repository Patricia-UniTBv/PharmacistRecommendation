using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers
{
    public sealed class EmailService : IEmailService
    {
        public async Task ComposeAsync(string subject,
                                       string body,
                                       IEnumerable<string> to,
                                       IEnumerable<string>? attachmentPaths = null)
        {
            var message = new EmailMessage
            {
                Subject = subject,
                Body = body,
                To = to.ToList()
            };

            if (attachmentPaths != null)
                foreach (var path in attachmentPaths.Where(File.Exists))
                    message.Attachments.Add(new EmailAttachment(path));

            await Email.Default.ComposeAsync(message);
        }
    }
}
