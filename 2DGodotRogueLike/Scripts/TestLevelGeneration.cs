using Godot;
using System;

public class TestLevelGeneration : Node2D
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";


  //declare Foreground and Background map variables
  public Godot.TileMap ForegroundMap;

  public Godot.TileMap FloodFillMap; 

  //range of 0 to 100 with step range of 5
  [Export(PropertyHint.Range,"0,100,1")]
  public int initialDeadChance;

  
  [Export(PropertyHint.Range,"1,8,1")]
  public int deathLimit;

  [Export(PropertyHint.Range,"1,8,1")]
  public int birthLimit;

  
  [Export(PropertyHint.Range,"1,100,1")]
  public int maxIterations;


  //for tileset
  [Export]
  public int TopTile;

  [Export]
  public int BottomTile;


  [Export]
  public Vector2 tileMapSize;

  int width;
  int height;

  Random random;


  //bounds of cell for neighbor check
  Vector2[] neighborsToCheck;

  int [,] terrainMap;


  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    //link forground and background map variables to the nodes

    random = new Random();

    //fill neighbors

    neighborsToCheck = new Vector2[8];
    int pos = 0;
    for(int i = -1; i <= 1; ++i)  
    {
      for(int j = -1; j <= 1; ++j)  
      {
        if(i == 0 && j == 0)
          continue;

        neighborsToCheck[pos++] = new Vector2(i, j);
      }
    }

    ForegroundMap = GetNode("ForegroundMap") as TileMap;
    FloodFillMap = GetNode("FloodFillMap") as TileMap;

    runSimulation(maxIterations/2, true);
    runSimulation((maxIterations + 1)/2, false);

  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
      if(Input.IsActionJustPressed("GenerateMap"))
      {
        runSimulation(maxIterations, true);
      }

      if(Input.IsActionJustPressed("IterateMap"))
      {
        runSimulation(maxIterations, false);
      }


  }


  //clears the map
  public void clearMap()
  {
    ForegroundMap.Clear();
    FloodFillMap.Clear();
    terrainMap = null;

  }


  //seeds the terrain map with random dead or alive values
  private void initPositions()
  {
    for(int x = 0; x < width; ++x)
    {
      for(int y = 0; y < height; ++y)
      {

        //if less than chance them dead or then alive
        if(random.Next(1,101) < initialDeadChance)
        {
          //alive
          terrainMap[x,y] = 1;

        }
        else
        {
          //alive
          terrainMap[x,y] = 0;
        }
      }
    }
  }


  //runs simulation
  public void runSimulation(int iterations, bool newMap)
  {
    width = (int) tileMapSize.x;
    height = (int) tileMapSize.y;

    if(newMap)
    {
      clearMap();
      terrainMap = new int[width, height];
      initPositions();
    }



    //run for set number of iterations
    for(int i = 0; i < iterations; ++i)  
    {
      terrainMap = neighborCheck(terrainMap);
    }

    //We now have a generated level


    //Create a flood fill map 


    for(int x = 0; x < width; ++x)
    {
      for(int y = 0; y < height; ++y)
      {
        if(terrainMap[x,y] == 1)
        {
          //range of -x to x amd -y to y to center the tile map;
          //set to the top tile
          ForegroundMap.SetCell(-x + width / 2, -y + width / 2, TopTile);
        }
        //create a border of walkable terrain around the entire map
        else if(x == 0 || y == 0 || x == width || y == height)
        {
          ForegroundMap.SetCell(-x + width / 2, -y + width / 2, TopTile);

        }
        else
        {
          ForegroundMap.SetCell(-x + width / 2, -y + width / 2, BottomTile);

        }

        //set the background to all bottom tile????
        //BackgroundMap.SetCell(-x + width / 2, -y + width / 2, BottomTile);
      }
    }

    //update bitmask for auto tile
    ForegroundMap.UpdateBitmaskRegion(new Vector2(-width/2, -height/2), new Vector2(width/2, height/2));

      

  }

  private int[,] neighborCheck( int[,] oldTerrainMap)
  {
    //more memory???
    int[,] newTerrainMap = new int[width,height];

    int numNeighbors = 0;
    
    for(int x = 0; x < width; ++x)
    {
      for(int y = 0; y < height; ++y)
      {

        //Get Number of Neighbors
        numNeighbors = 0;
        //for each neighbor
        foreach (Vector2 tilePos in neighborsToCheck)
        {
          //break on out of bounds
          if(x + tilePos.x < 0 || x + tilePos.x >= width || y + tilePos.y < 0 || y + tilePos.y >= height)
          {
            //count the border as alive so + 1
            numNeighbors++;
            continue;
          }
          else  //if not out of bounds
          {
            //neighbor adds the value of either dead or alive neighbor
            numNeighbors += oldTerrainMap[x + (int)tilePos.x, y + (int)tilePos.y];
          }
        }  



        //game of life rules

        //if alive
        if(oldTerrainMap[x,y] == 1)
        {
          //if num neighbors < death limit than die)
          if(numNeighbors < deathLimit)
          {
            newTerrainMap[x,y] = 0;
          }
          else  //above death limit 
          {
            newTerrainMap[x,y] = 1;
          }
        }
        else  //if zero
        {
          //if num neighbors < death limit than die)
          if(numNeighbors > birthLimit)
          {
            //create new cell
            newTerrainMap[x,y] = 1;
          }
          else  //below birth limit 
          {
            //stays dead
            newTerrainMap[x,y] = 0;
          }
        }
      }
    }

    //return the new map
    return newTerrainMap;
  }
}
