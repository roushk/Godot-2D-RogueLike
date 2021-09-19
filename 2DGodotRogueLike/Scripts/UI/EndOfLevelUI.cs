using Godot;
using System;

public class EndOfLevelUI : Control
{
  public TestLevelGeneration testLevelGeneration;
  public PlayerManager playerManager;

  public void _on_ExitGameButton_pressed()
  {
    //Close the game
    GetTree().Quit();
  }
  
  //Spawn new level
  public void _on_ContinueGameButton_pressed()
  {
    testLevelGeneration.GenAllAndSpawnOreAndPlayer();
  }

  public override void _Ready()
  {
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    testLevelGeneration = GetNode<TestLevelGeneration>("/root/TestLevelGenNode");
  }

}
