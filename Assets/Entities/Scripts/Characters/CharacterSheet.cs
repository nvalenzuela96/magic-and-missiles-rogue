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
        public float Strength { get; set; } = 0f;
        public float Constitution { get; set; } = 0f;
        public float Intelligence { get; set; } = 0f;
        public float MaxHealth { get; set; } = 100f;
        public float CurrentHealth { get; set; } = 100f;
        public float MaxMana { get; set; } = 0f;
        public float CurrentMana { get; set; } = 0f;
        public float Armor { get; set; } = 0f;
        public float Speed { get; set; } = 0f;
        public float MeleeDamage { get; set; } = 0f;
    }
}
