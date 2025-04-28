using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class TemperatureMonitoring
{
    public int MonitoringId { get; set; }

    public decimal? Temperature { get; set; }

    public virtual Monitoring Monitoring { get; set; } = null!;
}
