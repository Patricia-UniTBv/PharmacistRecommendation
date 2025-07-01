using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class MedicationDocument
{
    public int Id { get; set; }

    public int MedicationId { get; set; }

    public int DocumentId { get; set; }

    public virtual Document Document { get; set; } = null!;

    public virtual Medication Medication { get; set; } = null!;
}
