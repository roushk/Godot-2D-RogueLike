using Godot;
using System;

public class Forge : Interactable
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    base._Ready();
  }

  public override void StartInteract()
	{
    base.StartInteract();

    Console.WriteLine("Player Started Interacting with Anvil");
    //Spawn Player Crafting UI
	}

	//Called when player ends interaction with the object
	public override void EndInteract()
	{
    base.EndInteract();
    Console.WriteLine("Player Stopped Interacting with Anvil");
	}

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
    UpdateSelf(delta);
  }
}
