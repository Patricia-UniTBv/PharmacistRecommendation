using System;
using System.Collections.Generic;

namespace Entities.Models;  // FIXED: Changed from PharmacistRecommendation.Models

public partial class Pharmacist
{
    public int Id { get; set; }

    public bool? Active { get; set; }

    public bool? ActivationCheck { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Ncm { get; set; } = null!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();

    public virtual User IdNavigation { get; set; } = null!;
}
