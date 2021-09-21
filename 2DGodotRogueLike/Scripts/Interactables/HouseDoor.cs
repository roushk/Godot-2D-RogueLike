using Godot;
using System;

public class HouseDoor : Interactable
{
  //Sets the scene to this new scene
  PackedScene HouseIndoorScene = (PackedScene)ResourceLoader.Load("res://Scenes/Levels/GeneratedLevel.tscn");

  bool flippedSprite = false;
  bool startedInteract = false;

  //Time before the item interacts
  float timeLeft = 1.0f;


  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    base._Ready();
    interactionRadius *= 1.2f;
    if(flippedSprite)
      animatedSprite.FlipH = true;
  }

  public override void StartInteract()
	{
    //Switch to opened door
    //Play door opening sound
    //Animate player walking into the doorway
    //etc
    animatedSprite.Frame = 0;
    startedInteract = true;

	}
  void EnterDoorway()
  {
    base.StartInteract();
    playerManager.ChangeLevelTo(HouseIndoorScene);
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

    if(startedInteract)
    {
      timeLeft -=delta;
      if(timeLeft <= 0)
      {
        EnterDoorway();
      }
    }
  }
}
