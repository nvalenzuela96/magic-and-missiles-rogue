using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using System;
using System.Xml.Linq;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Characters
{
    public class Equipment
    {
        public Equippable Head { get; set; } = new Equippable()
        { 
            Name = "None",
            Description = "",
            EquipmentType = EquipmentTypes.Head ,
            MainStat = new StatModifier()
            {
                StatType = StatTypes.Armor,
                Value = 0
            }
        };

        public Equippable Body { get; set; } = new Equippable()
        {
            Name = "None",
            Description = "",
            EquipmentType = EquipmentTypes.Body,
            MainStat = new StatModifier()
            {
                StatType = StatTypes.Armor,
                Value = 0
            }
        };

        public Equippable Melee { get; set; } = new Equippable()
        {
            Name = "None",
            Description = "",
            EquipmentType = EquipmentTypes.Melee,
            MainStat = new StatModifier()
            {
                StatType = StatTypes.WeaponDamage,
                Value = 1
            }
        };
    }
}