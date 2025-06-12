using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public record DiabetesMonitoringDTO(
      DateTime Date,
      decimal? BloodGlucose,
      decimal? Height,
      decimal? Weight);
}
