using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class PharmacyCard
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public int PatientId { get; set; }

    public string? Code { get; set; }

    public DateTime IssueDate { get; set; }

    public virtual ICollection<Monitoring> Monitorings { get; set; } = new List<Monitoring>();

    public virtual Patient Patient { get; set; } = null!;

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
