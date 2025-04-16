using System;
using System.Collections.Generic;

namespace Pharmacy.Data.Models;

public partial class Doctor
{
    public int Id { get; set; }

    public string LastName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string? Specialization { get; set; }

    public virtual ICollection<DoctorMedication> DoctorMedications { get; set; } = new List<DoctorMedication>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
