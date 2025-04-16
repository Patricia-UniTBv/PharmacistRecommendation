using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Monitoring
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? CardId { get; set; }

    public double Value { get; set; }

    public string Parameter { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime MonitoringDate { get; set; }

    public virtual PharmacyCard? Card { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}
