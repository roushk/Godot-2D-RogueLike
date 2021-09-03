using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TestLevelGeneration : Node2D
{

	public CCLGenerator CCLGen = new CCLGenerator();
	public WFCSimpleTiledModel WFCSTM = new WFCSimpleTiledModel();

	//General Signals
	public void GenerateNewTileMapButton_Callback()
	{
		GD.Print("Clicked Generate New Tile Map Button");
		GenerateMap(maxIterations, true);
		CCLGen.UpdateInternalMap(width, height, ref terrainMap);
		WFCSTM.UpdateInternalMap(width, height, ref terrainMap);
		UpdateMapData();
	}

	public void ClearMapButton_Callback()
  {
		GD.Print("Clicked Clear Map Button");
		ClearMap();
	}

	//CCL Signals
	public void CCL_GenerateCompleteMapButton_Callback()
	{
		GenerateMap(maxIterations, true);
		CCLGen.UpdateInternalMap(width, height, ref terrainMap);
		CCLGen.CCLAlgorithm();
		UpdateMapData();
	}

	public void CCL_IterateSimulationOnce_Callback()
	{
		GD.Print("Clicked Prune Tile Map Button");
		GenerateMap(1, false);
	  CCLGen.UpdateInternalMap(width, height, ref terrainMap);
		UpdateMapData();
	}

	public void CCL_GenerateCaveGroups_Callback()
	{
		GD.Print("Generate Cave Groups Button");
		CCLGen.CCLAlgorithm();
		UpdateMapData();
	}

	public void CCL_ViewRootGroups_Callback()
	{
		GD.Print("Clicked ViewRootGroups_Callback Button");
		CCLGen.VisualizeIDTree(CCLGenerator.VisualizeMode.Root);
	}

	public void CCL_ViewIndividualGroups_Callback()
	{
		GD.Print("Clicked ViewRootGroups_Callback Button");
		CCLGen.VisualizeIDTree(CCLGenerator.VisualizeMode.Individual);
	}

	public void CCL_SelectLargestCave_Callback()
	{
		GD.Print("Clicked CCL_SelectLargestCave_Callback Button");
		//CCLGen.SelectLargestCave();
		List<KeyValuePair<int, int>> largestSet = CCLGen.GetLargestSet();
		GD.Print("Largest Set Count = " + largestSet.Count);
	}

	//WFC Signals
	public void WFC_GenerateCompleteMapButton_Callback()
	{
		GenerateMap(maxIterations, true);
		WFCSTM.UpdateInternalMap(width, height, ref terrainMap);
		//WFCSTM.CCLAlgorithm();
		UpdateMapData();
	}

	public void WFC_IterateSimulationOnce_Callback()
	{
		GD.Print("Clicked Prune Tile Map Button");
		GenerateMap(1, false);
		WFCSTM.UpdateInternalMap(width, height, ref terrainMap);
		UpdateMapData();
	}

	public PackedScene IDColorMapScene = ResourceLoader.Load<PackedScene>("res://TemplateScenes/IDAndColorUIElement.tscn");
	Node MapGenColorListNode;

	//!!!!!!!!!!!!!!!!!!!!!!!!
	//map  0,0 = bottom right
	//!!!!!!!!!!!!!!!!!!!!!!!!

  //Map is 
  //Top Tile = 1 = Stone Ground
  //Bottom Tile = 0 = Grass Wall
  //8,9,10,11,12 for quadrants
  //0,2,3 for extra quadrants

  //declare Foreground and Background map variables
  public Godot.TileMap ForegroundMap;

  public Godot.TileMap mapIDVisualizationRef; 

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
  public int TestTile;

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
		MapGenColorListNode = GetTree().Root.FindNode("MapGenColorList/VBoxContainer2");

		//link forground and background map variables to the nodes

		random = new Random();

		//fill neighbors offset for any arbitrary vector, precalced into a container
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
		mapIDVisualizationRef = GetNode("FloodFillMap") as TileMap;

		GenerateMap(maxIterations, true);

		CCLGen.SetVisualizationMap(ref mapIDVisualizationRef);
		
		//CCLGen.UpdateInternalMap(width, height, ref terrainMap);

		//CCLGen.CCLAlgorithm();
		UpdateMapData();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {

  }

  //clears the map
  public void ClearMap()
  {
		ForegroundMap.Clear();
		CCLGen.Clear();
		terrainMap = null;
  }

	//Creates a new map of the tileMapSize and iterates the game of life a set number of times
	private void GenerateMap(int iterations, bool newMap)
	{
		width = (int) tileMapSize.x;
		height = (int) tileMapSize.y;

		if(newMap)
		{
			ClearMap();
			terrainMap = new int[width, height];
			initPositions();
		}

		//run for set number of iterations
		for(int i = 0; i < iterations; ++i)  
		{
			terrainMap = GameOfLifeIterate(terrainMap);
		}

		//We now have a generated level

		//add border
		//create a border of walkable Stone Ground terrain around the entire map
		for(int y = 0; y < height; ++y)
		{
			for(int x = 0; x < width; ++x)
			{  
				if(x == 0 || y == 0 || x == width - 1 || y == height - 1)
				{
					terrainMap[x,y] = 1;
				}
			}
		}
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

	//Sets the foreground map to the tile data of the terrain map
	private void UpdateMapData()
	{
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
					ForegroundMap.SetCell(-x + width / 2, -y + height / 2, TopTile);
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

		//ForegroundMap.SetCell(width / 2, width / 2,TestTile);
	}

	//Runs game of life a single iteration
  private int[,] GameOfLifeIterate( int[,] oldTerrainMap)
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
