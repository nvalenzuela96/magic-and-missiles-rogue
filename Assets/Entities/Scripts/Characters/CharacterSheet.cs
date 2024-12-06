using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Characters
{
    public class CharacterSheet
    {
        public string Name { get; set; }
        public Stat Strength { get; set; } = new Stat(1f);
        public Stat Constitution { get; set; } = new Stat(1f);
        public Stat Intelligence { get; set; } = new Stat(1f);
        public Stat MaxHealth { get; set; } = new Stat(100f);
        public float CurrentHealth { get; set; } = 100f;
        public Stat MaxMana { get; set; } = new Stat(100f);
        public float CurrentMana { get; set; } = 100f;
        public Stat Armor { get; set; } = new Stat(1f);
        public Stat Speed { get; set; } = new Stat(1f);
        public Stat WeaponDamage { get; set; } = new Stat(1f);
        public Equipment Equipment { get; set; } = new Equipment();
    }
}
