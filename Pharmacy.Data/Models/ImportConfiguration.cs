using System;
using System.Collections.Generic;

namespace Pharmacy.Data.Models;

public partial class ImportConfiguration
{
    public int Id { get; set; }

    public int PharmacyId { get; set; }

    public string? PrescriptionPath { get; set; }

    public string? ImportPath { get; set; }

    public virtual Pharmacy Pharmacy { get; set; } = null!;
}
