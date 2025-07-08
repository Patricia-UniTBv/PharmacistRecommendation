using System;
using System.Collections.Generic;

namespace PharmacistRecommendation.Models;

public partial class Assistant
{
    public int Id { get; set; }

    public int SupervisorPharmacistId { get; set; }

    public virtual User IdNavigation { get; set; } = null!;

    public virtual Pharmacist SupervisorPharmacist { get; set; } = null!;
}
