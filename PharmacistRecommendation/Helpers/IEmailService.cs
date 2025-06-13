using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacistRecommendation.Helpers
{
    public interface IEmailService
    {
        Task ComposeAsync(string subject,
                          string body,
                          IEnumerable<string> to,
                          IEnumerable<string>? attachmentPaths = null);
    }
}
