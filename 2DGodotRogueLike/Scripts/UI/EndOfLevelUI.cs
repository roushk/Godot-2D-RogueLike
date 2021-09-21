using Godot;
using System;

public class EndOfLevelUI : Control
{
  public TestLevelGeneration testLevelGeneration;
  public PlayerManager playerManager;

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
    testLevelGeneration.GenAllAndSpawnOreAndPlayer(resetPlayerOnContinue);

    //Reset after used the reset
    resetPlayerOnContinue = false;
  }

  public override void _Ready()
  {
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");

    //TODO map manager 
    testLevelGeneration = GetNode<TestLevelGeneration>("/root/TestLevelGenNode");
  }

}
