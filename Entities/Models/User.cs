using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class User
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? Email { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Phone { get; set; }

    public string? PersonalId { get; set; }

    public string? Role { get; set; }

    public string? Ncm { get; set; }

    public virtual Assistant? Assistant { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual Pharmacist? Pharmacist { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
