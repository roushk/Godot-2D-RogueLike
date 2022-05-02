using Godot;
using System;

public class Interactable : Spatial
{

  [Export]
  public float interactionRadius = 20.0f;

  protected PlayerManager playerManager;
  protected Random random = new Random();
  protected bool inRange = false;

  public AnimatedSprite animatedSprite;
  public ShaderMaterial shaderMaterial;

  public bool playerInteracting = false;

  public bool toggleableInteractable = false;
  
  bool ModulateColorDebug = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    shaderMaterial = animatedSprite.Material as ShaderMaterial;
  }

  protected void StartSparkling()
  {
    if(shaderMaterial != null)
    {
      shaderMaterial.SetShaderParam("blinking", true);
    }
  }

  protected void StopSparkling()
  {
    if(shaderMaterial != null)
    {
      shaderMaterial.SetShaderParam("blinking", false);
    }
  }

  public void EnteredInteractionRadius()
  {
    StartSparkling();
    inRange = true;
    //Enable Interaction
    playerManager.topDownPlayer.interactablesInRange.Add(this);

    //if(ModulateColorDebug)
    //  Modulate = new Color(0.5f,1,0.5f,1);
  }

  public void ExitedInteractionRadius()
  {
    StopSparkling();
    if(playerManager.topDownPlayer != null)
      playerManager.topDownPlayer.interactablesInRange.Remove(this);

    //if(ModulateColorDebug)
    //  Modulate = new Color(1,1,1,1);
    inRange = false;
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

    //Don't run end interaction if player isn't interacting and is like walking around with the key down
    if(!playerInteracting)
      return;

    if(!toggleableInteractable)
    {
      playerInteracting = false;
    }

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
  public override void _PhysicsProcess(float delta)
  {
    float distanceToPlayerSquared = float.MaxValue;

    if(playerManager.topDownPlayer != null)
      distanceToPlayerSquared = playerManager.topDownPlayer.Translation.DistanceSquaredTo(Translation);

    //If player within aggro radius
    if(distanceToPlayerSquared < interactionRadius * interactionRadius)
    {
      if(inRange == false)
      {
        EnteredInteractionRadius();
      }
      //Do nothing if already in range
    }
    //else if interactionR
    else if(inRange)  //Else if not within range and we think we are then remove from the list
    {
      ExitedInteractionRadius();
    }
  }
}
