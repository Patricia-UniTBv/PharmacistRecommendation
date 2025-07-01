using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class EmailConfiguration
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? SmtpServer { get; set; }

    public string? SmtpPort { get; set; }

    public bool EnableSsl { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
