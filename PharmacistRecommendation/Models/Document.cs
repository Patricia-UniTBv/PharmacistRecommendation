using System;
using System.Collections.Generic;

namespace PharmacistRecommendation.Models;

public partial class Document
{
    public int Id { get; set; }

    public int? PatientId { get; set; }

    public int DocumentTypeId { get; set; }

    public string? FilePath { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UserId { get; set; }

    public virtual DocumentType DocumentType { get; set; } = null!;

    public virtual ICollection<MedicationDocument> MedicationDocuments { get; set; } = new List<MedicationDocument>();

    public virtual Patient? Patient { get; set; }

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual User? User { get; set; }
}
