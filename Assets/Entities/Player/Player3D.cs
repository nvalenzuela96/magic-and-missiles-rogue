using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Characters;
using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System;
using System.Collections;
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

    public float currentHp;
	public float currentMana;

    private Camera3D camera;
	private SpringArm3D cameraBoom;
	private Node3D cameraPivot;

	private Vector3 cameraRotation;
	private Vector3 defaultCameraRotation;

	private Camera3D pov;

	private CollisionShape3D playerCollider;
	private MeshInstance3D playerMesh;

	private World world;
	private HUD hud;

	public ArrayList lootingList = new();

    public Mob target;
	public Mob spellTarget;
	List<Mob> attackerList;

	public CharacterSheet characterSheet = new();

    Timer attackTimer;
    Timer castTimer;

    public List<Spell> spellList = new();
    public Spell castingSpell;

    bool cameraPanned = false;
	public bool inCombat = false;
    bool withinRange = false;
    bool attackChambered = false;
	bool casting = false;

	bool actionStarted = false;
	double timer = 0;
	double thresholdTimer = 0.2;

    public override void _Ready()
	{
		cameraPivot = GetNode<Node3D>("CameraPivot");
		cameraBoom = GetNode<SpringArm3D>("CameraPivot/CameraBoom");
		camera = GetNode<Camera3D>("CameraPivot/CameraBoom/Camera");
		defaultCameraRotation = cameraPivot.RotationDegrees;

        ReadyCharacterSheet();

        pov = GetNode<Camera3D>("CharacterPOV");

		world = GetParent<World>();

		hud = GetNode<HUD>("HUD");
		hud.GetNode<Label>("PlayerUnitFrame/Grid/Name").Text = characterSheet.Name;

        playerCollider = GetNode<CollisionShape3D>("PlayerCollider");
		playerMesh = GetNode<MeshInstance3D>("PlayerMesh");

		attackTimer = GetNode<Timer>("AttackTimer");
		castTimer = GetNode<Timer>("CastTimer");

        cameraBoom.SpringLength = zoomStart;

		currentHp = characterSheet.CurrentHealth;
		currentMana = characterSheet.CurrentMana;

        PutOnEquipment(world.equippables.First(e => e.Name == "Hat"));
        PutOnEquipment(world.equippables.First(e => e.Name == "Jacket"));
        PutOnEquipment(world.equippables.First(e => e.Name == "Knife"));
		ReadyInventory();

		spellList.Add(new Spell());
		hud.ReadySpells();

        attackerList = new();
    }

	private void ReadyCharacterSheet()
	{
		characterSheet = new()
		{
			Name = "Tigginz"
		};
	}

	private void ReadyInventory()
	{
		var sword = world.equippables.First(e => e.Name == "Sword");

        characterSheet.inventory.Items.Add(sword);
        hud.itemList.AddItem(sword.Name, GD.Load<Texture2D>(sword.Icon));

        foreach (var item in world.consumables)
		{
			hud.itemList.AddItem(item.Name, GD.Load<Texture2D>(item.Icon));
			characterSheet.inventory.Items.Add(item);
		}
	}

	public void PutOnEquipment(Equippable equipment)
	{
		foreach (var property in characterSheet.Equipment.GetType().GetProperties())
		{
            if (property.Name == equipment.EquipmentType.ToString())
			{
				GD.Print(equipment.Name);
				Equippable oldEquipped = (Equippable)characterSheet.Equipment.GetType().GetProperty(property.Name).GetValue(characterSheet.Equipment);
				RemoveEquipment(oldEquipped);
                property.SetValue(characterSheet.Equipment, equipment);
				AddStatModifierToPlayer(equipment.MainStat);
				foreach(var statModifier in equipment.StatModifiers)
				{
					if (statModifier != null)
					{
                        AddStatModifierToPlayer(statModifier);
                    }
				}
			}
		}
	}

	public void RemoveEquipment(Equippable equipment)
	{
		RemoveStatModifierFromPlayer(equipment.MainStat);
		foreach(var statModifier in equipment.StatModifiers)
		{
            if (statModifier != null)
            {
                RemoveStatModifierFromPlayer(statModifier);
            }
        }
		if (equipment.Name != "None")
		{
			characterSheet.inventory.Items.Add(equipment);
			hud.itemList.AddItem(equipment.Name, GD.Load<Texture2D>(equipment.Icon));
        }
        hud.ReadyCharacterSheetPanel();
    }

    private void RemoveStatModifierFromPlayer(StatModifier statModifier)
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
	
	private void AddStatModifierToPlayer(StatModifier statModifier)
	{
		if (statModifier.StatType == StatTypes.CurrentHealth)
		{
			characterSheet.CurrentHealth += statModifier.Value;
		}
		else
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
        hud.ReadyCharacterSheetPanel();
    }

	public void UseConsumable(Consumable item)
	{
        AddStatModifierToPlayer(item.MainStat);
        foreach (var statModifier in item.StatModifiers)
        {
            if (statModifier != null)
            {
                AddStatModifierToPlayer(statModifier);
            }
        }
    }

	private void Loot(Godot.Collections.Dictionary result)
	{
        var collision = result.GetValueOrDefault("collider");
		if (collision.Obj.GetType() == typeof(Mob))
		{
            var lootTarget = (Mob)collision.Obj;
            if (lootTarget.lootable)
            {
                foreach (var item in lootTarget.dropTable)
                {
                    if (item.GetType() == typeof(Consumable))
                    {
                        Consumable consumable = (Consumable)item;
						GD.Print(consumable.Name);
                        hud.lootList.AddItem(consumable.Name, GD.Load<Texture2D>(consumable.Icon));
                    }
                }
				lootingList = lootTarget.dropTable;
                hud.lootPanel.Visible = true;
            }
			else
			{
				GD.Print("Target is alive!");
			}
        }
    }

	private void UpdateStats()
	{
		currentHp = characterSheet.CurrentHealth;
		if (currentHp > characterSheet.MaxHealth.Value)
		{
			currentHp = characterSheet.MaxHealth.Value;
		}
		currentMana = characterSheet.CurrentMana;
        if (currentMana > characterSheet.MaxMana.Value)
        {
            currentMana = characterSheet.MaxMana.Value;
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
        if (Input.IsActionJustPressed("action_bar_1"))
        {
			CastSpell(spellList[0]);
        }
		if (Input.IsActionJustPressed("quick_save"))
		{
			var file = JsonConvert.SerializeObject(characterSheet);
			GD.Print(file);
		}
	}

	public Godot.Collections.Dictionary Click()
	{
		GD.Print("Cast Ray");
		var mousePosition = GetViewport().GetMousePosition();
        var from = camera.ProjectRayOrigin(mousePosition);
        var to = from + camera.ProjectRayNormal(mousePosition) * 1000f;
        var space = GetWorld3D().DirectSpaceState;
        var rayCast = PhysicsRayQueryParameters3D.Create(to, from);
        rayCast.From = from;
        rayCast.To = to;
        var result = space.IntersectRay(rayCast);

		return result;
    }

    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		hud.UpdateUnitFrame();
		UpdateStats();

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
			hud.castBar.MaxValue = castingSpell.CastTime;
			hud.castBar.Value = castTimer.TimeLeft;
		}

		if (Input.IsActionJustPressed("camera_pan"))
		{
			actionStarted = true;
		}
		if (Input.IsActionPressed("camera_pan") && actionStarted)
		{
			timer += delta;
		}
		if (timer >= thresholdTimer && actionStarted)
		{ 
			actionStarted = false;
			timer = 0;
		}
		
		if (Input.IsActionJustReleased("camera_pan"))
		{
			if (timer < thresholdTimer && actionStarted)
			{
                var result = Click();
				var collision = result.GetValueOrDefault("collider");
				if (collision.Obj?.GetType() == typeof(Mob))
				{
                    if (result.Count > 0)
                    {
                        HandleTargetting(collision);
                    }
                }
                else
                {
                    target = null;
                    hud.targetUnitFrame.Hide();
                }
            }
			actionStarted = false;
			timer = 0;
		}
    }

	private void UpdateTargetUnitFrame()
	{
        hud.targetHealthBar.Value = target.currentHp;
        hud.targetManaBar.Value = target.currentMana;
        hud.GetNode<Label>("TargetUnitFrame/Grid/Name").Text = target.name;
    }

	private void CastSpell(Spell spell)
	{
		if (target != null && !casting)
		{
			spellTarget = target;
			casting = true;
			castingSpell = spell;
			castTimer.WaitTime = spell.CastTime;
			hud.castBar.Visible = true;
			castTimer.Start();
			GD.Print($"Casting {spell.SpellName}...");
		}
	}

	private void HandleTargetting(Variant collision)
	{
        target = (Mob)collision.Obj;
        hud.targetHealthBar.MaxValue = target.maxHealthPoints;
        hud.targetHealthBar.Value = target.currentHp;
        hud.targetManaBar.MaxValue = target.maxManaPoints;
        hud.targetManaBar.Value = target.currentMana;
        target.GetTargetted(this);
        hud.targetUnitFrame.Show();
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
            if (!(Input.IsActionPressed("camera_pan") || Input.IsActionPressed("mouse_steer")))
            {
				Vector3 playerFacing = new Vector3(cameraPivot.RotationDegrees.X, defaultCameraRotation.Y, defaultCameraRotation.Z);
                cameraPivot.RotationDegrees = playerFacing;
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
		spellDraw.target = spellTarget;
		spellDraw.caster = this;
		world.AddChild(spellDraw);
		spellDraw.Transform = Transform;
		castingSpell = null;
		hud.castBar.Visible = false;
    }

    public void TakeDamage(float damageAmount)
    {
        characterSheet.CurrentHealth -= damageAmount;
		hud.healthBar.Value = currentHp;
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
