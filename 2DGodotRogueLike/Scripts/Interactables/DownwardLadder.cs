using Godot;
using System;

public class DownwardLadder : Interactable
{


  public override void _Ready()
  {
    base._Ready();
  }

  public override void StartInteract()
	{
    base.StartInteract();
    playerManager.topDownPlayer.currentlySelectedUI = PlayerTopDown.CurrentlySelectedUI.EndLevelUI;
    //Close the game on interact
    //Popup UI to be like "Do you want Descend??"
    //GetTree.Quit();
    //Spawn Player Crafting UI
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
