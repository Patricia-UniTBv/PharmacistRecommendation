using System;
using System.Collections.Generic;

namespace Pharmacy.Data.Models;

public partial class Assistant
{
    public int Id { get; set; }

    public int SupervisorPharmacistId { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual User IdNavigation { get; set; } = null!;

    public virtual Pharmacist SupervisorPharmacist { get; set; } = null!;
}
