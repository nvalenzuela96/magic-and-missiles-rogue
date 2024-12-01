using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using System;

namespace MagicandMissilesRogue.Assets.Entities.Scripts.Characters
{
    public class Equipment
    {
        public Equippable Head { get; set; }
        public Equippable Body { get; set; }
        public Equippable Melee { get; set; }
    }
}