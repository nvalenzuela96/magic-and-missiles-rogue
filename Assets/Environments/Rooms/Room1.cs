using Godot;
using System;
using System.Collections.Generic;

public partial class Room1 : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        RandomizeRooms(this.GetNode<Node3D>("Room"));
	}

	public void RandomizeRooms(Node3D room)
	{
        GD.Print(room.Name);
        var newRoomChildren = room.GetChildren();
        foreach (var child in newRoomChildren)
        {
            if (child.GetType() == typeof(Area3D))
            {
                if (child.Name.ToString().Contains("To"))
                {
                    GD.Print(child.Name.ToString());
					var childArea = (Area3D)child;
                    var randNum = GD.RandRange(2, 3);
                    var connectingRoomScene = GD.Load<PackedScene>($"res://Assets/Environments/Rooms/Room{randNum}.tscn");
                    var instance = connectingRoomScene.Instantiate();
                    AddChild(instance);
                    var newRoom = GetNode<Node3D>(instance.Name.ToString());
					newRoom.GlobalTransform = childArea.GlobalTransform;
                    newRoom.GlobalRotation = childArea.GlobalRotation;
					RandomizeRooms(newRoom.GetNode<Node3D>("Room"));
                }
            }
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
