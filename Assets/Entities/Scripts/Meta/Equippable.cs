using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Meta
{
    public partial class Equippable : Item
    {
        public EquipmentTypes EquipmentType { get; set; }
        public string Stat {  get; set; }
        public float Value { get; set; }
    }
}
