using Godot;

public partial class Mob : CharacterBody3D
{
    [Export]
    float speed = 2f;
    [Export]
    float meleeRange = 2f;
    [Export]
    float meleeDamage = 5f;
    [Export]
    float healthPoints = 100f;
    [Export]
    float manaPoints = 100f;
    [Export]
    float attackSpeed = 3f;

    Area3D aggroRange;
    Timer timer;
    Player3D target;

    bool aggroed = false;
    bool withinRange = false;
    bool attackChambered = false;

    public string name = "Lola's enemy.";

    public override void _Ready()
    {
        base._Ready();
        aggroRange = GetNode<Area3D>("AggroRange");
        timer = GetNode<Timer>("CombatTimer");
        timer.WaitTime = attackSpeed;
    }

    public override void _Process(double delta)
    {
        if (!aggroed)
        {
            AggroFromIdle();
        }
        else
        {
            GetInCombatRange();
        }
        if (attackChambered && withinRange)
        {
            GD.Print("Swing at target.");
            attackChambered = false;
        }
    }

    public void AggroFromIdle()
    {
        if (aggroRange.GetOverlappingBodies().Count > 0)
        {
            if (aggroRange.GetOverlappingBodies()[0] is Player3D)
            {
                target = (Player3D)aggroRange.GetOverlappingBodies()[0];
                aggroed = true; 
                target.inCombat = true;
            }
        }
    }

    public void GetInCombatRange()
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
            if (timer.IsStopped())
            {
                timer.Start();
            }
        }
        else
        {
            withinRange = false;
        }
    }

    public void _OnCombatTimerTimeout()
    {
        attackChambered = true;
    }
}