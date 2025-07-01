using System;
using System.Collections.Generic;

namespace PharmacistRecommendation.Models;

public partial class Monitoring
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? CardId { get; set; }

    public string? Notes { get; set; }

    public DateTime MonitoringDate { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public string? ParametersJson { get; set; }

    public virtual PharmacyCard? Card { get; set; }

    public virtual Patient Patient { get; set; } = null!;
}
