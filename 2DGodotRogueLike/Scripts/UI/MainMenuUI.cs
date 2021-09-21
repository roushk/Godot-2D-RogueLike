using Godot;
using System;

public class MainMenuUI : Control
{
  PackedScene generatedLevelScene = (PackedScene)ResourceLoader.Load("res://Scenes/Levels/Town.tscn");
  PlayerManager playerManager;

  public override void _Ready()
  {
    //Setup player manager
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
  }

  public void _on_ExitGameButton_pressed()
  {
    //Close the game
    GetTree().Quit();
  }
  
  //Spawn new level
  public void _on_StartGameButton_pressed()
  {
    //need to change scene, cannot add it to current root as its control
    playerManager.ChangeLevelTo(generatedLevelScene, "/root/TownRootNode/PlayerSpawnLocation_MaxwellsHouse");
    //Reset after used the reset
  }

}
