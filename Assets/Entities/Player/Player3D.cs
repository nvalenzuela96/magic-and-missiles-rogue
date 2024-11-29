using Godot;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;

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
	float healthPoints = 100f;
	[Export]
	float manaPoints = 100f;

	[Export]
	float meleeRange = 2f;
    [Export]
    float meleeDamage = 5f;
    [Export]
    float attackSpeed = 3f;

    private Camera3D camera;
	private SpringArm3D cameraBoom;
	private Node3D cameraPivot;

	private Vector3 cameraRotation;

	private Camera3D pov;

	private CollisionShape3D playerCollider;
	private MeshInstance3D playerMesh;

	private World world;
	private HUD hud;

    Mob target;

	Timer timer;

    bool cameraPanned = false;
	public bool inCombat = false;
    bool withinRange = false;
    bool attackChambered = false;

    public override void _Ready()
	{
		cameraPivot = GetNode<Node3D>("CameraPivot");
		cameraBoom = GetNode<SpringArm3D>("CameraPivot/CameraBoom");
		camera = GetNode<Camera3D>("CameraPivot/CameraBoom/Camera");

		pov = GetNode<Camera3D>("CharacterPOV");

		world = GetParent<World>();
		hud = GetNode<HUD>("HUD");
		
		playerCollider = GetNode<CollisionShape3D>("PlayerCollider");
		playerMesh = GetNode<MeshInstance3D>("PlayerMesh");

		timer = GetNode<Timer>("AttackTimer");

		cameraBoom.SpringLength = zoomStart;
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
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.Pressed)
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
                    var collision = result.GetValueOrDefault("collider");
                    if (collision.Obj.GetType() == typeof(Mob))
                    {
                        target = (Mob)collision.Obj;
                        GD.Print(target.name);
                    }
                }
			}
		}
	}

    public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);

        if (attackChambered && withinRange)
        {
            GD.Print("Player swing at target.");
            attackChambered = false;
        }
        HandleMovement(delta);
		if (target != null)
		{
			CheckRangeToTarget();
        }
	}

	private async void HandleMovement(double delta)
    {
		Vector3 velocity = Velocity;

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
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}

	private void CheckRangeToTarget()
	{
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
            GD.Print("Target out of range");
        }
    }

	private void _OnAttackTimerTimeout()
	{
        attackChambered = true;
    }
}
