using System;
using System.Collections.Generic;

namespace PharmacistRecommendation.Models;

public partial class Pharmacy
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Cui { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Logo { get; set; }

    public string? ConsentTemplate { get; set; }

    public virtual ICollection<EmailConfiguration> EmailConfigurations { get; set; } = new List<EmailConfiguration>();

    public virtual ICollection<ImportConfiguration> ImportConfigurations { get; set; } = new List<ImportConfiguration>();

    public virtual ICollection<PharmacyCard> PharmacyCards { get; set; } = new List<PharmacyCard>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
