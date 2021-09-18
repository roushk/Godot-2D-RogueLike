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

  public bool playerInteracting = false;

  public bool toggleableInteractable = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    testLevelGeneration = GetNode<TestLevelGeneration>("/root/TestLevelGenNode");
    animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;
  }

  

	//Called when player starts interaction with the object
	public virtual void StartInteract()
	{
    if(!toggleableInteractable)
    {
      playerInteracting = true;
    }
	}

	//Called when player ends interaction with the object
	public virtual void EndInteract()
	{
    //if toggleable then do it on the end of an interaction
    if(toggleableInteractable && playerInteracting)
    {
      playerInteracting = false;
    }
    else if(toggleableInteractable && !playerInteracting)
    {
      playerInteracting = true;
    }

    //Dont run end interaction if player isn't interacting and is like walking around with the key down
    if(!playerInteracting)
      return;

    if(!toggleableInteractable)
      playerInteracting = false;

	}

  public override void _PhysicsProcess(float delta)
  {
    
  }

  public void ObjectBeingRemoved()
  {
    //Safely remove this object from the player's interactables if they exist 
    if(playerManager.topDownPlayer.closestInteractable == this)
      playerManager.topDownPlayer.closestInteractable = null;
    
    if(playerManager.topDownPlayer.currentlyInteractingWith == this)
      playerManager.topDownPlayer.currentlyInteractingWith = null;

    playerManager.topDownPlayer.interactablesInRange.Remove(this);
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
      if(inRange == false)
      {
        inRange = true;
        //Enable Interaction
        playerManager.topDownPlayer.interactablesInRange.Add(this);
        Modulate = new Color(0.5f,1,0.5f,1);
      }
      //Do nothing if already in range
    }
    //else if interactionR
    else if(inRange)  //Else if not within range and we think we are then remove from the list
    {
      playerManager.topDownPlayer.interactablesInRange.Remove(this);
      Modulate = new Color(1,1,1,1);
      inRange = false;
    }
  }
}
