using Godot;
using System;

public class EndOfLevelUI : Control
{
  public TestLevelGeneration testLevelGeneration;
  public PlayerManager playerManager;

  PackedScene newGameLevel = (PackedScene)ResourceLoader.Load("res://Scenes/Levels/Town.tscn");

  public bool resetPlayerOnContinue = false;

  public void _on_ExitGameButton_pressed()
  {
    //Close the game
    GetTree().Quit();
  }
  
  //Spawn new level
  public void _on_ContinueGameButton_pressed()
  {
    //TODO change to MapManager.ContinueGame() or something
    playerManager.ChangeLevelTo(newGameLevel);
    Visible = false;

    //Reset after used the reset
    resetPlayerOnContinue = false;
  }

  public override void _Ready()
  {
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
  }

}
