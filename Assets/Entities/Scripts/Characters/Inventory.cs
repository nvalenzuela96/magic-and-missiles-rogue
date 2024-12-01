using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Characters
{
    public class Inventory
    {
        public int Slots { get; set; }
        public List<Item> Items { get; set; }
    }
}
