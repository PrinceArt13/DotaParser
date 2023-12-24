using System;
using System.Collections.Generic;

namespace DotaParser.Models.Entities;

public partial class Hero
{
    public Guid HeroId { get; set; }

    public string Name { get; set; } = null!;

    public int Health { get; set; }

    public int Mana { get; set; }

    public double Armor { get; set; }

    public double MagicResistance { get; set; }

    public int Damage { get; set; }

    public int MoveSpeed { get; set; }

    public bool AttackType { get; set; }

    public Guid AttributeId { get; set; }

    public virtual MainAttribute Attribute { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
