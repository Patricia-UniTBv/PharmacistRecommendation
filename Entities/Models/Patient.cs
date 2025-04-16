using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Patient
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? PersonalId { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual ICollection<Monitoring> Monitorings { get; set; } = new List<Monitoring>();

    public virtual ICollection<PharmacyCard> PharmacyCards { get; set; } = new List<PharmacyCard>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
