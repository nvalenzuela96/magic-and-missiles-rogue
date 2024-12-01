using Godot;
using System;

public partial class MainMenu : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _OnStartPressed()
	{
		ResourceLoader.Load("res://Assets/Environments/World.tscn");
		GetTree().ChangeSceneToFile("res://Assets/Environments/World.tscn");
	}

	public void _OnSettingsPressed()
	{

	}

	public void _OnQuitPressed()
	{

	}
}
