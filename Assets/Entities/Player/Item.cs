using Godot;
using System;

namespace MagicandMissilesRogue.Assets.Entities.Player
{
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemTypes Type { get; set; }
        public string Stat { get; set; }
        public float Value { get; set; }
    }
}