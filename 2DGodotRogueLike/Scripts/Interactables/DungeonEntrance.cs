using Godot;
using System;

public class DungeonEntrance : Interactable
{

  PackedScene generatedLevelScene = (PackedScene)ResourceLoader.Load("res://Scenes/Levels/GeneratedLevel.tscn");

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
	base._Ready();
	interactionRadius *= 4f;
  }

  public override void StartInteract()
	{
	base.StartInteract();
	GetTree().ChangeSceneTo(generatedLevelScene);
	}

	//Called when player ends interaction with the object
	public override void EndInteract()
	{
	base.EndInteract();
	}

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
	base._PhysicsProcess(delta);
  }
}
