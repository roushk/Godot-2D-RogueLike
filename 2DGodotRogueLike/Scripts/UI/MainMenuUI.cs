using Godot;
using System;

public class MainMenuUI : Control
{
  PackedScene generatedLevelScene = (PackedScene)ResourceLoader.Load("res://Scenes/Levels/GeneratedLevel.tscn");

  public void _on_ExitGameButton_pressed()
  {
    //Close the game
    GetTree().Quit();
  }
  
  //Spawn new level
  public void _on_StartGameButton_pressed()
  {
    //need to change scene, cannot add it to current root as its control
    GetTree().ChangeSceneTo(generatedLevelScene);
    //Reset after used the reset
  }

}
