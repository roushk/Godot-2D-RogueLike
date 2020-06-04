using Godot;
using System;

public class TestLevelGeneration : Node2D
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";


//declare Foreground and Background map variables
  public TileMap ForegroundMap;

  public TileMap BackgroundMap; 


  [Export(PropertyHint.Range,"0,100,5")]
  public int chanceOfInitialDeadChance;
  

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    //link forground and background map variables to the nodes
    ForegroundMap = GetNode("ForegroundMap") as TileMap;
    BackgroundMap = GetNode("BackgroundMap") as TileMap;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      
  }
}
