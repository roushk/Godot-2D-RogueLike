using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
public class TestLevelGeneration : Node2D
{


  //Map is 
  //Top Tile = 1 = Stone Ground
  //Bottom Tile = 0 = Grass Wall
  //8,9,10,11,12 for quadrants
  //0,2,3 for extra quadrants

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

    //add border
    //create a border of walkable Stone Ground terrain around the entire map
    for(int y = 0; y < height; ++y)
    {
      for(int x = 0; x < width; ++x)
      {  
        if(x == 0 || y == 0 || x == width || y == height)
        {
          terrainMap[x,y] = 1;
        }
      }
    }

    //Create a flood fill map 


    //create a flood fill map the same size as the actual map
    int [,] floodFillMap = new int[width, height];

    //Going from top left to bottom right
    //from an arbitary pixel p
    //get the pixel to the left and to the top of p
    // ? X ? 
    // X p ?
    // ? ? ?
    //loop over terrain map
   
    //This long line is a dictionary from Id's to a hashset (unique list) of integer x,y coords as a keypair
    Dictionary<KeyValuePair<int, int>, int> dictIDToListofCoords = new Dictionary<KeyValuePair<int, int>, int>();

    List<HashSet<int>> AdjacentSets = new List<HashSet<int>>();

    //dictionary of int ID of set to ID of parent set 
    //key = ID of node, value = ID of parent
    Dictionary<int,int> IDToParent = new Dictionary<int, int>();


    //ID to size for the first iteration
    int [] sizesIT1 = new int [1000];


    //starting ID is 1 so any with id zero have no parent
    int currentID = 1;
    for(int y = 0; y < height; ++y)
    {
      for(int x = 0; x < width; ++x)
      {  
        //ignore walls
        if(terrainMap[x,y] != 1)
          continue;
          
        int IDA = 0;
        int IDB = 0;
        bool IDAExists = dictIDToListofCoords.TryGetValue(new KeyValuePair<int, int>(x - 1, y), out IDA);
        bool IDBExists = dictIDToListofCoords.TryGetValue(new KeyValuePair<int, int>(x, y - 1), out IDB);
        
        int AcurrentParentID = 0;
        bool AhasParent = IDToParent.TryGetValue(IDA, out AcurrentParentID);
        
        int BcurrentParentID = 0;
        bool BhasParent = IDToParent.TryGetValue(IDB, out BcurrentParentID);

        if(IDAExists && IDBExists)
        {
          //Parent is always smaller than child
          if(IDA < IDB)
          {
            //it doesnt have the parent or it has 
            if(!BhasParent || (BhasParent && IDA < BcurrentParentID))
            {
              IDToParent[IDB] = IDA;
            }
          }
          else if(IDB > IDA)
          {
            if(!AhasParent || (AhasParent && IDB < AcurrentParentID))
            {
              IDToParent[IDA] = IDB;
            }
            
          }
          //else dont set same ID to child of itself

          //IDA size > IDB size
          if(sizesIT1[IDA] > sizesIT1[IDB])
          {

            //Add to the larger set
            sizesIT1[IDB]++;
            dictIDToListofCoords.Add(new KeyValuePair<int, int>(x,y), IDB);
          }
          else //IDA size <= IDB size
          {
            //Add to the larger Set
            sizesIT1[IDA]++;
            dictIDToListofCoords.Add(new KeyValuePair<int, int>(x,y), IDA);
          }
        }
        //now we decide to add to IDA first if possible
        else if(IDAExists && !IDBExists) //redundant !IDBExists for clarity
        {
          sizesIT1[IDA]++;
          //if left key than set to left key
          dictIDToListofCoords.Add(new KeyValuePair<int, int>(x,y), IDA);
        }
        //If cannot get left key than check up key
        else if(IDBExists && !IDAExists)  //redundant !IDAExists for clarity
        {
          sizesIT1[IDB]++;
          //if up key than set to up key
          dictIDToListofCoords.Add(new KeyValuePair<int, int>(x,y), IDB);
        }
        //if cannot get left OR up key than create new key
        else
        {
          dictIDToListofCoords.Add(new KeyValuePair<int, int>(x,y), currentID++);
          sizesIT1[currentID] = 1;
        }
      }
    }
    Console.WriteLine("Max ID = " +  currentID.ToString());


    /*
    bool changed = true;
    while(changed)
    {
      changed = false;
      for(int i = 0; i < AdjacentSets.Count; ++i)
      {
        if(changed)
          break;

        for(int j = 0; j < AdjacentSets.Count; ++j)
        {
          if(i == j)
            continue;
          
          //If the intersect contains any overlap than combine the sets
          if(AdjacentSets.ElementAt(i).Intersect(AdjacentSets.ElementAt(j)).Any())
          {
            Console.WriteLine("Merging Sets");

            //combine sets
            AdjacentSets.ElementAt(i).UnionWith(AdjacentSets.ElementAt(j));

            Console.WriteLine("I is" + i.ToString());
            Console.WriteLine("J is" + j.ToString());

            //remove other set
            AdjacentSets.RemoveAt(j);
            changed = true;
            break;
          }
        }      
      }
    }
    */

    //put all the pixels into this new map so that we have the links between each pixel and each set still
    Dictionary<KeyValuePair<int, int>, int> listOfCoords = new Dictionary<KeyValuePair<int, int>, int>();

    //for every pixel
    for(int i = 0; i < dictIDToListofCoords.Count; ++i)
    {
      
      var element = dictIDToListofCoords.ElementAt(i);
      var key = element.Key;
      var value = element.Value;


      int initialID = element.Value;
      int ID = element.Value;
      int IDout = 0;
      //while we can get parents follow the path
      //Get Root
      while(IDToParent.TryGetValue(ID, out IDout))
      {
        //Console.WriteLine("Following Tree " + ID.ToString() + " ->" + IDout.ToString());
        //follow tree up
        ID = IDout;
      }

      //Console.WriteLine("Final ID = " + ID.ToString());
      //move this 1 pixel to its parent size
      sizesIT1[ID]++;
      sizesIT1[initialID]--;

      //add the sets together
      listOfCoords.Add(key,ID);
      //here when this set has no parent
      
      FloodFillMap.SetCell(-key.Key + width / 2, -key.Value + width / 2, ID);
      
    }

    for(int i = 0; i < 1000; ++i)
    {
      if(sizesIT1[i] > 0)
      {
        Console.WriteLine("Size of ID " + i.ToString() + " Is " + sizesIT1[i].ToString());
      }
    }


    //set the flood fill map according to the data 
    /*
    for(int i = 0; i < dictIDToListofCoords.Count; ++i)
    {
      var element = dictIDToListofCoords.ElementAt(i);
      var key = element.Key;
      var value = element.Value;
      //FloodFillMap.SetCell(-key.Key + width / 2, -key.Value + width / 2, value);
      
    }
    */

    //Update the cells
    for(int x = 0; x < width; ++x)
    {
      for(int y = 0; y < height; ++y)
      {


        //Top Tile = 1 = Stone Ground
        if(terrainMap[x,y] == 1)
        {
          //range of -x to x amd -y to y to center the tile map;
          //set to the top tile
          ForegroundMap.SetCell(-x + width / 2, -y + width / 2, TopTile);
        }
        else  //Bottom Tile = 0 = Grass Wall
        {
          ForegroundMap.SetCell(-x + width / 2, -y + height / 2, BottomTile);

        }

        //set the background to all bottom tile????
        //BackgroundMap.SetCell(-x + width / 2, -y + width / 2, BottomTile);
      }
    }

    //ForegroundMap.SetCell(-width / 2, -height / 2, 0);
    //oregroundMap.SetCell(0, 0, 1);
    //ForegroundMap.SetCell( width / 2, -(width - 1)  + height / 2, 0);


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
