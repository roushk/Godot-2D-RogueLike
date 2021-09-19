using Godot;
using System;

//Goal of this class is a single ref point for the player and player related stuff that is level agnostic

//Get the Player Manager
//playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
public class PlayerManager : Node
{  
  public PlayerTopDown topDownPlayer;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    
  }

  //Do this later, for now can just teleport player object
  //public void SavePlayerData()
  //{
  //}

  //Sets the player ref
  public void SetTopDownPlayer(ref PlayerTopDown player)
  {
    topDownPlayer = player;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    
  }
}
