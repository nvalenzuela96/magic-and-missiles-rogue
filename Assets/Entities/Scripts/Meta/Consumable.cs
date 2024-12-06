using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Meta
{
    public partial class Consumable : Item
    {
        public StatModifier MainStat {  get; set; }
        public List<StatModifier> StatModifiers { get; set; } = new List<StatModifier>();
        public string Icon { get; set; } = "res://Assets/UI/Icons/missing-icon.png";
    }
}
