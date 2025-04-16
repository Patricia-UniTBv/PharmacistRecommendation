using System;
using System.Collections.Generic;

namespace Pharmacy.Data.Models;

public partial class Document
{
    public int Id { get; set; }

    public int? PharmacistId { get; set; }

    public int? AssistantId { get; set; }

    public int? PatientId { get; set; }

    public int DocumentTypeId { get; set; }

    public string? FilePath { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual Assistant? Assistant { get; set; }

    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual ICollection<MedicationDocument> MedicationDocuments { get; set; } = new List<MedicationDocument>();

    public virtual Patient? Patient { get; set; }

    public virtual Pharmacist? Pharmacist { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
