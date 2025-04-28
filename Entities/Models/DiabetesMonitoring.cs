using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class DiabetesMonitoring
{
    public int MonitoringId { get; set; }

    public decimal? Glucose { get; set; }

    public virtual Monitoring Monitoring { get; set; } = null!;
}
