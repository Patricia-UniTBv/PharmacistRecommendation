using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class DocumentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
