using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Meta
{
    public class Stat
    {
        public float BaseValue;

        private readonly List<StatModifier> statModifiers;

        public Stat(float baseValue)
        {
            BaseValue = baseValue;
            statModifiers = new List<StatModifier>();
        }

        public float Value { get { return CalculateFinalValue(); } }

        public void AddModifier(StatModifier mod)
        {
            statModifiers.Add(mod);
        }

        public bool RemoveModifier(StatModifier mod)
        {
            return statModifiers.Remove(mod);
        }

        private float CalculateFinalValue()
        {
            float finalValue = BaseValue;

            for (int i = 0; i < statModifiers.Count; i++)
            {
                finalValue += statModifiers[i].Value;
            }
            // Rounding gets around dumb float calculation errors (like getting 12.0001f, instead of 12f)
            // 4 significant digits is usually precise enough, but feel free to change this to fit your needs
            return (float)Math.Round(finalValue, 4);
        }
    }

    public class StatModifier
    {
        public float Value { get; set; }
        public StatTypes StatType { get; set; }
    }
}
