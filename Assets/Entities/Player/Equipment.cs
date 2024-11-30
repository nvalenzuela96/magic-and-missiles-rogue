using Godot;
using System;

namespace MagicandMissilesRogue.Assets.Entities.Player
{
    public class Equipment
    {
        public Item Head { get; set; } = new Item();
        public Item Body { get; set; } = new Item();
        public Item Melee { get; set; } = new Item();
    }
}