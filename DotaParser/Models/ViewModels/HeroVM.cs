using DotaParser.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotaParser.Models.ViewModels
{
    public class HeroVM
    {
        public string Name { get; set; } = null!;

        public int Health { get; set; }

        public int Mana { get; set; }

        public double Armor { get; set; }

        public double MagicResistance { get; set; }

        public int Damage { get; set; }

        public int MoveSpeed { get; set; }

        public bool AttackType { get; set; }
    }
}
