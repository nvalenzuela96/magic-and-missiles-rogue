using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Characters;
using System;

public partial class Spell : CharacterBody3D
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
    [Export]
    public bool AttackSpell { get; set; } = true;

    public Mob target;
    public Player3D caster;
    Area3D area;

    public override void _Ready()
    {
        base._Ready();
        area = GetNode<Area3D>("Area3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (area.GetOverlappingBodies().Count > 0)
        {
            if (area.GetOverlappingBodies()[0] == target)
            {
                target.TakeDamage(Damage, caster);
                GD.Print($"{target.name} hit by {caster.characterSheet.Name} with {SpellName} for {Damage}!");
                QueueFree();
            }
        }

        Velocity = Position.DirectionTo(target.Position) * 60f;
        MoveAndSlide();
    }
}
