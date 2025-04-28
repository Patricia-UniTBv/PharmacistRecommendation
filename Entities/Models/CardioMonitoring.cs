using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class CardioMonitoring
{
    public int MonitoringId { get; set; }

    public decimal? MaxBloodPressure { get; set; }

    public decimal? MinBloodPressure { get; set; }

    public int? HeartRate { get; set; }

    public decimal? PulseOximetry { get; set; }

    public virtual Monitoring Monitoring { get; set; } = null!;
}
