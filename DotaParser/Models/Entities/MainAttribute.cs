using System;
using System.Collections.Generic;

namespace DotaParser.Models.Entities;

public partial class MainAttribute
{
    public Guid AttributeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Hero> Heroes { get; set; } = new List<Hero>();
}
