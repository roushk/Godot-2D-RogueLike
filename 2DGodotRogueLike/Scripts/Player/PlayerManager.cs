using Godot;
using System;

//Goal of this class is a single ref point for the player and player related stuff that is level agnostic

//Get the Player Manager
//playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
public class PlayerManager : Node
{  

  private PackedScene playerObjectScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player/TopDownPlayerScene.tscn");
  private PackedScene playerCameraAndUI = ResourceLoader.Load<PackedScene>("res://Scenes/Player/TopDownPlayerCameraAndUI.tscn");

  public PlayerTopDown topDownPlayer;

  //TODO move player inventory here
  public Inventory playerInventory;

  public Inventory playerTownInventory;
  
  public Camera2D playerCamera;

  bool createPlayerAndCamera = true;
  bool selectedLevelToChangeTo = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    playerInventory = new Inventory(this);
    playerTownInventory = new Inventory(this);

    //check for player camera and player
    if(GetTree().CurrentScene.FindNode("Player") != null)
    {
      topDownPlayer = GetTree().CurrentScene.FindNode("Player") as PlayerTopDown;
    }

    if(GetTree().CurrentScene.FindNode("PlayerCamera") != null)
    {
      playerCamera = GetTree().CurrentScene.FindNode("PlayerCamera") as Camera2D;
    }
  }

  //Do this later, for now can just teleport player object
  //public void SavePlayerData()
  //{
  //}

  string nodeToSpawnPlayerAt = "/root/TownRootNode/PlayerSpawnLocation_MaxwellsHouse";

  //Changes the level to a new scene, also can set a node to search for to set the players initial pos to
  public void ChangeLevelTo(PackedScene newScene, string _nodeToSpawnPlayerAt = "")
  {
    if(topDownPlayer != null)
      topDownPlayer.QueueFree();
    if(playerCamera != null)
      playerCamera.QueueFree();
  
    topDownPlayer = null;
    playerCamera = null;
    GetTree().ChangeSceneTo(newScene);
    createPlayerAndCamera = true;
    nodeToSpawnPlayerAt = _nodeToSpawnPlayerAt;
    selectedLevelToChangeTo = true;
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

  //PlayerSpawnLocation_MaxwellsHouse


  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    if(createPlayerAndCamera && selectedLevelToChangeTo)
    {
      selectedLevelToChangeTo = false;

      //Add player camera to the scene
      playerCamera = playerCameraAndUI.Instance<Camera2D>();
      GetTree().Root.AddChild(playerCamera);

      //Add player to the scene
      topDownPlayer = playerObjectScene.Instance<PlayerTopDown>();
      GetTree().Root.AddChild(topDownPlayer);
      //Load player data as this is only when changing levels
      createPlayerAndCamera = false;

      if(nodeToSpawnPlayerAt != "")
      {
        if(GetNode(nodeToSpawnPlayerAt) == null)
        {
          Console.WriteLine("Cannot find node " + nodeToSpawnPlayerAt);
          throw new Exception ("Cannot find node " + nodeToSpawnPlayerAt);
        }
        topDownPlayer.GlobalPosition = (GetNode(nodeToSpawnPlayerAt) as Node2D).GlobalPosition;
        playerCamera.GlobalPosition = (GetNode(nodeToSpawnPlayerAt) as Node2D).GlobalPosition;
      }
    }
  }
}
