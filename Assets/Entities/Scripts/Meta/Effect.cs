using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Meta
{
    public class Effect
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public EffectTypes Type { get; set; }
        public float Value { get; set; }
        public bool Display { get; set; }
        public float Time { get; set; }
        public float TimeRemaining { get; set; }
    }
}
