using Godot;
using System;

public partial class Spell : Node
{
    [Export]
    public string SpellName { get; set; } = "Fireball";
    [Export]
    public string SpellDescription { get; set; } = "Blast an mf with a fireball";
    [Export]
    public float CastTime { get; set; } = 3f;
    [Export]
    public bool IsInstant { get; set; } = false;
    [Export]
    public float Damage { get; set; } = 20f;
}
