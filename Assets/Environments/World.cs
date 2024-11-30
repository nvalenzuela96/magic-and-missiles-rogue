using Godot;
using MagicandMissilesRogue.Assets.Entities.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public partial class World : Node3D
{
	public List<Item> items;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SeedData();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void SeedData()
	{
		string fileName = "res://Assets/Entities/Json/Items.json";
		string jsonString = FileAccess.GetFileAsString(fileName);
		items = JsonConvert.DeserializeObject<List<Item>>(jsonString);
		foreach (Item item in items)
		{ 
			GD.Print(item.Name);
		}
	}
}
