using Godot;
using System;

public class Interactable : Node2D
{

  [Export]
  public float interactionRadius = 20.0f;
  protected bool inRange = false;

  //TODO move out this to a MapManager singleton
  public TestLevelGeneration testLevelGeneration;
  protected PlayerManager playerManager;

  public AnimatedSprite animatedSprite;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    testLevelGeneration = GetNode<TestLevelGeneration>("/root/TestLevelGenNode");
    animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;
  }


  public override void _PhysicsProcess(float delta)
  {
    UpdateSelf(delta);
  }
  // Called every frame. 'delta' is the elapsed time since the previous frame.
  protected  void UpdateSelf(float delta)
  {
    float distanceToPlayerSquared = float.MaxValue;

    if(playerManager.topDownPlayer != null)
      distanceToPlayerSquared = playerManager.topDownPlayer.GlobalPosition.DistanceSquaredTo(GlobalPosition);

    //If player within aggro radius
    if(distanceToPlayerSquared < interactionRadius * interactionRadius)
    {
      //Make UI visible

      inRange = true;
      //Enable Interaction
      playerManager.topDownPlayer.interactablesInRange.Add(this);
      animatedSprite.Modulate = new Color(0.5f,1,0.5f,1);
    }
    else if(inRange)  //Else if not within range and we think we are then remove from the list
    {
      playerManager.topDownPlayer.interactablesInRange.Remove(this);
      animatedSprite.Modulate = new Color(1,1,1,1);
    }
  }

}
