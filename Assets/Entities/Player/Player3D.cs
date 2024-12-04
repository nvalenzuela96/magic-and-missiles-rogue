using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Characters;
using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Godot.HttpRequest;

public partial class Player3D : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[Export(PropertyHint.Range, "0.1,1.0")]
	float mouseSensitivity = 0.3f;
	[Export(PropertyHint.Range, "-90,0,1")]
	float minPitch = -90f;
	[Export(PropertyHint.Range, "0,90,1")]
	float maxPitch = 90f;
	[Export]
	float zoomStart = 8f;
	[Export]
	float cameraSpeed = 3f;
    [Export]
    PackedScene spellProjectile;

    [Export]
    float meleeRange = 2f;

    float currentHp;
	float currentMana;

    private Camera3D camera;
	private SpringArm3D cameraBoom;
	private Node3D cameraPivot;

	private Vector3 cameraRotation;

	private Camera3D pov;

	private CollisionShape3D playerCollider;
	private MeshInstance3D playerMesh;

	private World world;
	private HUD hud;
	private ProgressBar healthBar;
	private ProgressBar manaBar;
	private ProgressBar castBar;

	private PanelContainer targetUnitFrame;
	private ProgressBar targetHealthBar;
	private ProgressBar targetManaBar;

	private PanelContainer charSheetPanel;

    public Mob target;
	List<Mob> attackerList;

	public CharacterSheet characterSheet = new();

    Timer attackTimer;
    Timer castTimer;

	public Spell castingSpell;

    bool cameraPanned = false;
	public bool inCombat = false;
    bool withinRange = false;
    bool attackChambered = false;
	bool casting = false;

    public override void _Ready()
	{
		cameraPivot = GetNode<Node3D>("CameraPivot");
		cameraBoom = GetNode<SpringArm3D>("CameraPivot/CameraBoom");
		camera = GetNode<Camera3D>("CameraPivot/CameraBoom/Camera");

        ReadyCharacterSheet();

        pov = GetNode<Camera3D>("CharacterPOV");

		world = GetParent<World>();

		hud = GetNode<HUD>("HUD");
		hud.GetNode<Label>("PlayerUnitFrame/Grid/Name").Text = characterSheet.Name;
		castBar = hud.GetNode<ProgressBar>("CastBar");

        healthBar = hud.GetNode<ProgressBar>("PlayerUnitFrame/Grid/HealthBar");
		manaBar = hud.GetNode<ProgressBar>("PlayerUnitFrame/Grid/ManaBar");

        targetUnitFrame = hud.GetNode<PanelContainer>("TargetUnitFrame");
        targetHealthBar = hud.GetNode<ProgressBar>("TargetUnitFrame/Grid/HealthBar");
        targetManaBar = hud.GetNode<ProgressBar>("TargetUnitFrame/Grid/ManaBar");

        playerCollider = GetNode<CollisionShape3D>("PlayerCollider");
		playerMesh = GetNode<MeshInstance3D>("PlayerMesh");

		attackTimer = GetNode<Timer>("AttackTimer");
		castTimer = GetNode<Timer>("CastTimer");

		cameraBoom.SpringLength = zoomStart;

		currentHp = characterSheet.CurrentHealth.Value;
		currentMana = characterSheet.CurrentMana.Value;
		healthBar.Value = currentHp;
		manaBar.Value = currentMana;

		charSheetPanel = hud.GetNode<PanelContainer>("CharacterSheet");

        PutOnEquipment(world.equippables.First(e => e.Name == "Hat"));
        PutOnEquipment(world.equippables.First(e => e.Name == "Jacket"));
        PutOnEquipment(world.equippables.First(e => e.Name == "Knife"));
        ReadyCharacterSheetPanel();

        attackerList = new();
    }

	private void ReadyCharacterSheet()
	{
		characterSheet = new()
		{
			Name = "Tigginz"
		};
	}

	private void ReadyCharacterSheetPanel()
	{
		charSheetPanel.GetNode<Label>("CSHContainer/Name").Text = characterSheet.Name;
		charSheetPanel.GetNode<Label>("CSHContainer/Head").Text += characterSheet.Equipment.Head.Name;
		charSheetPanel.GetNode<Label>("CSHContainer/Body").Text += characterSheet.Equipment.Body.Name;
		charSheetPanel.GetNode<Label>("CSHContainer/Armor").Text += characterSheet.Armor.Value;
		charSheetPanel.GetNode<Label>("CSHContainer/Melee").Text += characterSheet.Equipment.Melee.Name;
		charSheetPanel.GetNode<Label>("CSHContainer/MeleeDamage").Text += characterSheet.WeaponDamage.Value;
		charSheetPanel.GetNode<TextureButton>("EquipmentL/HeadSlot").TextureNormal = GD.Load<Texture2D>(characterSheet.Equipment.Head.Icon);
		charSheetPanel.GetNode<TextureButton>("EquipmentL/BodySlot").TextureNormal = GD.Load<Texture2D>(characterSheet.Equipment.Body.Icon);
		charSheetPanel.GetNode<TextureButton>("EquipmentContainer/MainHand").TextureNormal = GD.Load<Texture2D>(characterSheet.Equipment.Melee.Icon);
	}

	private void PutOnEquipment(Equippable equipment)
	{
		foreach (var property in characterSheet.Equipment.GetType().GetProperties())
		{
            if (property.Name == equipment.EquipmentType.ToString())
			{
				GD.Print(equipment.Name);
				Equippable oldEquipped = (Equippable)characterSheet.Equipment.GetType().GetProperty(property.Name).GetValue(characterSheet.Equipment);
				RemoveEquipment(oldEquipped);
                property.SetValue(characterSheet.Equipment, equipment);
				AddNewEquipmentStat(equipment.MainStat);
				foreach(var statModifier in equipment.StatModifiers)
				{
					if (statModifier != null)
					{
                        AddNewEquipmentStat(statModifier);
                    }
				}
			}
		}
	}

	private void RemoveEquipment(Equippable equipment)
	{
		RemoveOldEquipmentStat(equipment.MainStat);
		foreach(var statModifier in equipment.StatModifiers)
		{
            if (statModifier != null)
            {
                RemoveOldEquipmentStat(statModifier);
            }
        }
	}

    private void RemoveOldEquipmentStat(StatModifier statModifier)
	{
		foreach (var property in characterSheet.GetType().GetProperties())
		{
			if (property.Name == statModifier.StatType.ToString())
			{
				Stat stat = (Stat)characterSheet.GetType().GetProperty(property.Name).GetValue(characterSheet);
				stat.RemoveModifier(statModifier);
            }
		}
	}
	
	private void AddNewEquipmentStat(StatModifier statModifier)
	{
		foreach (var property in characterSheet.GetType().GetProperties())
		{
			if (property.Name == statModifier.StatType.ToString())
			{
				Stat stat = (Stat)characterSheet.GetType().GetProperty(property.Name).GetValue(characterSheet);
				stat.AddModifier(statModifier);
			}
		}
	}

	public override void _Input(InputEvent @event)
	{
		base._Input(@event);
		if (@event is InputEventMouseMotion motionEvent &&
			(Input.IsActionPressed("mouse_steer") || Input.IsActionPressed("camera_pan")))
		{
			if (Input.IsActionPressed("mouse_steer"))
			{
				Vector3 rotDeg = RotationDegrees;
				rotDeg.Y -= motionEvent.Relative.X * mouseSensitivity;
				RotationDegrees = rotDeg;

				rotDeg = cameraPivot.RotationDegrees;
				rotDeg.X -= motionEvent.Relative.Y * mouseSensitivity;
				rotDeg.X = Mathf.Clamp(rotDeg.X, minPitch, maxPitch);
				cameraPivot.RotationDegrees = rotDeg;
				cameraRotation = rotDeg;
			}
			if (Input.IsActionPressed("camera_pan"))
			{
				Vector3 rotDeg = cameraPivot.RotationDegrees;
				rotDeg.Y -= motionEvent.Relative.X * mouseSensitivity;
				rotDeg.X -= motionEvent.Relative.Y * mouseSensitivity;
				rotDeg.X = Mathf.Clamp(rotDeg.X, minPitch, maxPitch);
				cameraPivot.RotationDegrees = rotDeg;
			}
		}
		if (Input.IsActionJustPressed("camera_zoom_in"))
		{
			GD.Print("Zoom in");
			if (cameraBoom.SpringLength > 0.0)
			{
				GD.Print(cameraBoom.SpringLength);
				cameraBoom.SpringLength -= 1;
				GD.Print(cameraBoom.SpringLength);
			}
		}
		if (Input.IsActionJustPressed("camera_zoom_out"))
		{
			GD.Print("Zoom out");
			if (cameraBoom.SpringLength < 20.0)
			{
				GD.Print(cameraBoom.SpringLength);
				cameraBoom.SpringLength += 1;
				GD.Print(cameraBoom.SpringLength);
			}
		}
		if (Input.IsActionJustPressed("character_sheet"))
		{
			GD.Print(characterSheet.Equipment.Head.Name);
			GD.Print(characterSheet.Equipment.Body.Name);
			GD.Print(characterSheet.Equipment.Melee.Name);
			if (!charSheetPanel.Visible)
			{
                charSheetPanel.Visible = true;
            }
			else
			{
				charSheetPanel.Visible = false;
			}
        }

        if (Input.IsActionJustPressed("action_bar_1"))
        {
			CastSpell(new Spell());
        }

        if (@event is InputEventMouseButton mouseButton && Input.IsActionJustPressed("camera_pan"))
		{
            var from = camera.ProjectRayOrigin(mouseButton.Position);
            var to = from + camera.ProjectRayNormal(mouseButton.Position) * 1000f;
			var space = GetWorld3D().DirectSpaceState;
			var rayCast = PhysicsRayQueryParameters3D.Create(to, from);
			rayCast.From = from;
			rayCast.To = to;
            var result = space.IntersectRay(rayCast);
			if (result.Count > 0)
			{
				HandleTargetting(result);
            }
			else
			{
				target = null;
			}
		}
	}

    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

        if (attackChambered && withinRange && target != null)
        {
            GD.Print("Player swing at target.");
			target.TakeDamage(characterSheet.WeaponDamage.Value, this);
            attackChambered = false;
        }
        HandleMovement(delta);
		if (target != null)
		{
			CheckRangeToTarget();
			UpdateTargetUnitFrame();
        }
		if (casting)
		{
			castBar.MaxValue = castingSpell.CastTime;
			castBar.Value = castTimer.TimeLeft;
		}
	}

	private void UpdateTargetUnitFrame()
	{
        targetHealthBar.Value = target.currentHp;
        targetManaBar.Value = target.currentMana;
        hud.GetNode<Label>("TargetUnitFrame/Grid/Name").Text = target.name;
    }

	private void CastSpell(Spell spell)
	{
		if (target != null && !casting)
		{
			casting = true;
			castingSpell = spell;
			castTimer.WaitTime = spell.CastTime;
			castBar.Visible = true;
			castTimer.Start();
			GD.Print($"Casting {spell.SpellName}...");
		}
	}

	private void HandleTargetting(Godot.Collections.Dictionary result)
	{
        var collision = result.GetValueOrDefault("collider");
        if (collision.Obj.GetType() == typeof(Mob))
        {
            target = (Mob)collision.Obj;
            targetHealthBar.MaxValue = target.maxHealthPoints;
            targetHealthBar.Value = target.currentHp;
            targetManaBar.MaxValue = target.maxManaPoints;
            targetManaBar.Value = target.currentMana;
            target.GetTargetted(this);
            targetUnitFrame.Show();
        }
        else
        {
            target = null;
			targetUnitFrame.Hide();
        }
    }

	private async void HandleMovement(double delta)
    {
		Vector3 velocity = Velocity;

		var conditionalSpeed = Speed;
		if (casting)
		{
			conditionalSpeed = conditionalSpeed /= 3;
		}

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("left", "right", "forward", "back");

		if (inputDir != Vector2.Zero)
		{
            if (!Input.IsActionPressed("camera_pan"))
            {
                cameraPivot.RotationDegrees = cameraRotation;
            }
        }

		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * conditionalSpeed;
			velocity.Z = direction.Z * conditionalSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, conditionalSpeed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, conditionalSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void CheckRangeToTarget()
	{
        if (Position.DistanceTo(target.Position) <= meleeRange)
        {
            withinRange = true;
            if (attackTimer.IsStopped())
            {
                attackTimer.Start();
            }
        }
        else
        {
            withinRange = false;
        }
    }

	private void _OnAttackTimerTimeout()
	{
        attackChambered = true;
    }

	private void _OnCastTimerTimeout()
	{
        casting = false;
		Spell spellDraw = (Spell)spellProjectile.Instantiate();
		spellDraw.target = target;
		spellDraw.caster = this;
		world.AddChild(spellDraw);
		spellDraw.Transform = Transform;
		castingSpell = null;
		castBar.Visible = false;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHp -= damageAmount;
		healthBar.Value = currentHp;
        GD.Print($"Health is = {currentHp}");
        if (currentHp <= 0)
        {
            foreach (var attacker in attackerList)
            {
                attacker.target = null;
            }
			GD.Print("Player is dead!");
        }
    }

    public void GetTargetted(Mob enemy)
    {
        if (!attackerList.Contains(enemy))
        {
            attackerList.Add(enemy);
            GD.Print($"{enemy} added to attacker list.");
        }
    }
}
