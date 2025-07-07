using System;
using System.Collections.Generic;

namespace PharmacistRecommendation.Models;

public partial class Administrator
{
    public int Id { get; set; }

    public virtual User IdNavigation { get; set; } = null!;
}
