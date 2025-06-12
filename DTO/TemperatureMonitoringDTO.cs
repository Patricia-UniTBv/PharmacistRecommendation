using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public record TemperatureMonitoringDTO(
     DateTime Date,
     decimal? BodyTemperature,
     decimal? Height,
     decimal? Weight);
}
