using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

public partial class Medication
{
    public int Id { get; set; }

    [Column("CodCIM")]
    public string? CodCIM { get; set; }

    public string? Denumire { get; set; }

    [Column("DCI")]
    public string? DCI { get; set; }

    public string? FormaFarmaceutica { get; set; }

    public string? Concentratia { get; set; }

    public string? FirmaProducatoare { get; set; }

    public string? FirmaDetinatoare { get; set; }

    [Column("CodATC")]
    public string? CodATC { get; set; }

    public string? ActiuneTerapeutica { get; set; }

    public string? Prescriptie { get; set; }

    public string? NrData { get; set; }

    public string? Ambalaj { get; set; }

    public string? VolumAmbalaj { get; set; }

    public string? Valabilitate { get; set; }

    public string? Bulina { get; set; }

    public string? Diez { get; set; }

    public string? Stea { get; set; }

    public string? Triunghi { get; set; }

    public string? Dreptunghi { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
    [Column("PreviousCodCIM")]
    public string? PreviousCodCIM { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string DataSource { get; set; } = null!;

    public virtual ICollection<MedicationDocument> MedicationDocuments { get; set; } = new List<MedicationDocument>();

    public virtual ICollection<PrescriptionMedication> PrescriptionMedications { get; set; } = new List<PrescriptionMedication>();
}
