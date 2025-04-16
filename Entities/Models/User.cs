using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class User
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? PersonalId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int PharmacyId { get; set; }

    public string UserType { get; set; } = null!;

    public virtual Administrator? Administrator { get; set; }

    public virtual Assistant? Assistant { get; set; }

    public virtual Pharmacist? Pharmacist { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
