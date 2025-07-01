using System;
using System.Collections.Generic;

namespace Pharmacy.Data.Models;

public partial class Pharmacist
{
    public int Id { get; set; }

    public bool Active { get; set; }

    public bool ActivationCheck { get; set; }

    public virtual ICollection<Assistant> Assistants { get; set; } = new List<Assistant>();

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();

    public virtual User IdNavigation { get; set; } = null!;
}
