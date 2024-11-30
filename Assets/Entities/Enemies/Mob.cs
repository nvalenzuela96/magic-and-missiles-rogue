using Godot;
using System.Collections.Generic;

public partial class Mob : CharacterBody3D
{
    [Export]
    float speed = 2f;
    [Export]
    float meleeRange = 2f;
    [Export]
    float meleeDamage = 25f;
    [Export]
    public float maxHealthPoints = 100f;
    [Export]
    public float maxManaPoints = 100f;
    [Export]
    float attackSpeed = 3f;

    public float currentHp;
    public float currentMana;

    List<Player3D> attackerList;

    Area3D aggroRange;
    Timer timer;
    public Player3D target;

    bool aggroed = false;
    bool withinRange = false;
    bool attackChambered = false;

    bool alive = true;

    public string name = "Lola's Enemy";

    public override void _Ready()
    {
        base._Ready();
        aggroRange = GetNode<Area3D>("AggroRange");
        timer = GetNode<Timer>("CombatTimer");
        timer.WaitTime = attackSpeed;
        currentHp = maxHealthPoints;
        currentMana = maxManaPoints;
        attackerList = new List<Player3D>();
    }

    public override void _Process(double delta)
    {
        if (alive)
        {
            if (!aggroed)
            {
                AggroFromIdle();
            }
            else
            {
                if (target != null)
                {
                    GetInCombatRange();
                }
            }
            if (attackChambered && withinRange && target != null)
            {
                GD.Print("Swing at target.");
                target.TakeDamage(meleeDamage);
                attackChambered = false;
            }
        }
        else
        {
            RotationDegrees = new Vector3(90f, 0f, 0f);
        }
    }

    public void GetTargetted(Player3D player)
    {
        if (!attackerList.Contains(player))
        {
            attackerList.Add(player);
            GD.Print($"{player} added to attacker list.");
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
                target.GetTargetted(this);
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

    public void TakeDamage(float damageAmount, Player3D player)
    {
        if (!attackerList.Contains(player))
        {
            attackerList.Add(player);
            GD.Print($"{player} added to attacker list.");
        }
        currentHp -= damageAmount;
        GD.Print($"Health is = {currentHp}");
        if (currentHp <= 0)
        {
            currentHp = 0;
            GD.Print("I'm dead!");
            foreach (var attacker in attackerList)
            {
                GD.Print($"Attacker target{attacker.target}");
            }
            alive = false;
        }
        if (!aggroed)
        {
            aggroed = true;
            target = player;
        }
    }

    public void _OnCombatTimerTimeout()
    {
        attackChambered = true;
    }
}