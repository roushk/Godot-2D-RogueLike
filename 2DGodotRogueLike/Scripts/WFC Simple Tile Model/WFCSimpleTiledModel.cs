using Godot;
using System;

//Wave Function Collapse Simple Tiled Model Level Generator
public class WFCSimpleTiledModel : Node
{

  int [,] terrainMap;
  int width;
  int height;
  
  //Constraints
  
  
  public void UpdateMap(int _width, int _height, ref int [,] _terrainMap)
	{
		width = _width;
		height = _height;
		terrainMap = _terrainMap;
	}


  
  // Called when the node enters the scene tree for the first time.
  public void Iterate()
  {
      
  }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
