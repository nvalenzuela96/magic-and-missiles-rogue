using Godot;

public partial class Mob : CharacterBody3D
{
    [Export]
    float speed = 5.5f;
    [Export]
    float meleeRange = 2f;

    Area3D aggroRange;
    Player3D target;
    bool aggroed = false;
    bool withinRange = false;

    public override void _Ready()
    {
        base._Ready();
        aggroRange = GetNode<Area3D>("AggroRange");
    }

    public override void _Process(double delta)
    {
        if (aggroed == false)
        {
            AggroFromIdle();
        }
        else
        {
            AttackTarget();
        }
    }

    public void AggroFromIdle()
    {
        GD.Print("Checking aggro range...");
        if (aggroRange.GetOverlappingBodies().Count > 0)
        {
            GD.Print("Entity in range...");
            if (aggroRange.GetOverlappingBodies()[0] is Player3D)
            {
                GD.Print("Player in range. Aggro locked.");
                target = (Player3D)aggroRange.GetOverlappingBodies()[0];
                aggroed = true;
            }
        }
    }

    public void AttackTarget()
    {
        if (!withinRange)
        {
            LookAt(target.Position);
            Velocity = Position.DirectionTo(target.Position) * speed;
            MoveAndSlide();
        }
        if (Position.DistanceTo(target.Position) <= meleeRange)
        {
            withinRange = true;
        }
        else
        {
            withinRange = false;
        }
    }
}