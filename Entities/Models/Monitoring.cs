using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class Monitoring
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int? CardId { get; set; }

    public string? Notes { get; set; }

    public DateTime MonitoringDate { get; set; }

    public decimal? Height { get; set; }

    public decimal? Weight { get; set; }

    public virtual PharmacyCard? Card { get; set; }

    public virtual CardioMonitoring? CardioMonitoring { get; set; }

    public virtual DiabetesMonitoring? DiabetesMonitoring { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual TemperatureMonitoring? TemperatureMonitoring { get; set; }
}
