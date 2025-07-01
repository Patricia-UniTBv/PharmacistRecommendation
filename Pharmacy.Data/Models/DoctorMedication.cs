using System;
using System.Collections.Generic;

namespace Pharmacy.Data.Models;

public partial class DoctorMedication
{
    public int Id { get; set; }

    public int DoctorId { get; set; }

    public int MedicationId { get; set; }

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Medication Medication { get; set; } = null!;
}
