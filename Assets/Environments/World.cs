using Godot;
using MagicandMissilesRogue.Assets.Entities.Scripts.Characters;
using MagicandMissilesRogue.Assets.Entities.Scripts.Meta;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public partial class World : Node3D
{
	public List<Equippable> equippables;
	public List<Consumable> consumables;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SeedData();
		var playerScene = GD.Load<PackedScene>("res://Assets/Entities/Player/Player3D.tscn");
		var instance = playerScene.Instantiate();
		AddChild(instance);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SeedData()
	{
        var fileName = "res://Assets/Entities/Json/Items.json";
        var jsonString = FileAccess.GetFileAsString(fileName);
        equippables = JsonConvert.DeserializeObject<List<Equippable>>(jsonString);

        fileName = "res://Assets/Entities/Json/Consumables.json";
        jsonString = FileAccess.GetFileAsString(fileName);
        consumables = JsonConvert.DeserializeObject<List<Consumable>>(jsonString);
    }
}
