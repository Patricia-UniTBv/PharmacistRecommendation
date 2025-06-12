using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Parameters
{
    public record CardioParams(
      int? MaxBloodPressure,
      int? MinBloodPressure,
      int? HeartRate,
      int? PulseOximetry);
}
