using Godot;
using System;

//Goal of this class is a single ref point for the player and player related stuff that is level agnostic

//Get the Player Manager
//playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
public class PlayerManager : Node
{  
  public PlayerTopDown topDownPlayer;

  //TODO move player inventory here
  public Inventory playerInventory;

  public Inventory playerTownInventory;
  
  public Camera2D playerCamera;

  bool changedSceneThisFrame = false;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    Console.WriteLine("Initialized PlayerManager");

    playerInventory = new Inventory(this);
    playerTownInventory = new Inventory(this);
  }

  //Do this later, for now can just teleport player object
  //public void SavePlayerData()
  //{
  //}

  public void ChangeLevelTo(PackedScene newScene)
  {
    //SAVE PLAYER DATA HERE
    topDownPlayer.QueueFree();
    topDownPlayer = null;
    GetTree().ChangeSceneTo(newScene);
    changedSceneThisFrame = true;
  }

  //Sets the player ref
  public void SetTopDownPlayer(ref PlayerTopDown player)
  {
    topDownPlayer = player;
  }

  public void SetPlayerCamera(ref Camera2D _playerCamera)
  {
    playerCamera = _playerCamera;
  }


  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    if(changedSceneThisFrame)
    {
      //Load player data as this is only when changing levels
    }
  }
}
