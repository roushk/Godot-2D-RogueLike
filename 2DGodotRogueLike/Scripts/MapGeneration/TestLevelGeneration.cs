using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TestLevelGeneration : Node2D
{

#region Signals

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
		largestSet = CCLGen.GetLargestSet();
		UpdateMapData();
		ClearSmallerCaves();
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

	public void CCL_SelectLargestCave_Callback()
	{
		GD.Print("Clicked CCL_SelectLargestCave_Callback Button");
		//CCLGen.SelectLargestCave();
		largestSet = CCLGen.GetLargestSet();
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

	public void GenLargestAndAdjacency()
	{
		CCLGen.UpdateInternalMap(width, height, ref terrainMap);
		CCLGen.CCLAlgorithm();
		largestSet = CCLGen.GetLargestSet();
		UpdateMapData();
		ClearSmallerCaves();
		GenerateAdjacencyGrid();
	}

	public void Generate_CCL_Select_Largest_Adj()
	{
		GenerateMap(maxIterations, true);
		CCLGen.UpdateInternalMap(width, height, ref terrainMap);
		CCLGen.CCLAlgorithm();
		largestSet = CCLGen.GetLargestSet();
		UpdateMapData();
		ClearSmallerCaves();
		GenerateAdjacencyGrid();
	}

	//Sets visibility of the overlays
	public void OverlaySelected_Callback(int index)
	{
		if(index == 0)
		{
			foreach (var item in VisualizationMaps)
			{
				item.Value.Visible = false;
			}
		}
		else
		{
			foreach (var item in VisualizationMaps)
			{
				item.Value.Visible = false;
			}
			VisualizationMaps[ActiveOverlayOptions.GetItemText(index)].Visible = true;
		}
	}

#endregion

#region Variables
	public CCLGenerator CCLGen = new CCLGenerator();
	public WFCSimpleTiledModel WFCSTM = new WFCSimpleTiledModel();
	List<KeyValuePair<int, int>> largestSet = new List<KeyValuePair<int, int>>();

	public const int maxColors = 112;

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

  public Dictionary<string,Godot.TileMap> VisualizationMaps; 

  //range of 0 to 100 with step range of 5
  [Export(PropertyHint.Range,"0,100,1")]
  public int initialDeadChance;

  
  [Export(PropertyHint.Range,"1,8,1")]
  public int deathLimit;

  [Export(PropertyHint.Range,"1,8,1")]
  public int birthLimit;

  
  [Export(PropertyHint.Range,"1,100,1")]
  public int maxIterations;

	//Also displays it green, yellow, orange, red, purple, blue, cyan, white for 0,1,2,3,4,5,6,7
	List<Color> colorMap = new List<Color>{
		Colors.Green, 	//0
		Colors.Yellow,	//1
		Colors.Orange,	//2
		Colors.Red, 		//3
		Colors.Purple,	//4
		Colors.Blue, 		//5
		Colors.Cyan, 		//6
		Colors.White};	//7

  //for tileset
  [Export]
  public int TopTile;

  [Export]
  public int BottomTile;

  [Export]
  public int TestTile;

  [Export]
  public Vector2 tileMapSize;

  public int width { get; private set; }
  public int height { get; private set; }

  Random random;

  //bounds of cell for neighbor check, one for each radius 0 to n
	List<HashSet<Vector2>>  neighborsToCheck;
	Vector2[] neighborsToCheckSingle;

  int [,] terrainMap;

	//Dict of pixel to distance from closest wall
	Dictionary<KeyValuePair<int,int>, int> closestWalls = new Dictionary<KeyValuePair<int, int>, int>();

	OptionButton ActiveOverlayOptions;
#endregion

	public int [,] GetTerrainMapCopy()
	{
		int [,] newTerrainMap = new int[width, height];
		terrainMap.CopyTo(newTerrainMap,0);
		return newTerrainMap;
	}


	public void ClearSmallerCaves()
  {
    //Set every tile to wall
    for(int y = 0; y < height; ++y)
		{
			for(int x = 0; x < width; ++x)
			{  
				terrainMap[x,y] = 1;
			}
		}

		//Vector2 maxValues = new Vector2(0,0);
    //Set the current cave to floor
    foreach (var coord in largestSet)
    {
      terrainMap[coord.Key,coord.Value] = 0;

			//Code to find max values
			//if(coord.Key > maxValues.x)
			//{
			//	maxValues.x = coord.Key;
			//}
			//if(coord.Value > maxValues.y)
			//{
			//	maxValues.y = coord.Value;
			//}
    }

		//This is incase we want the max bounds of the cave to clear away extra floor tiles
		//for(int y = 0; y < height; ++y)
		//{
		//	for(int x = 0; x < width; ++x)
		//	{  
		//		if(x > maxValues.x + 1)
		//		{
		//
		//		}
		//		terrainMap[x,y] = 1;
		//	}
		//}
    UpdateMapData();
  }

	//based on https://www.geeksforgeeks.org/mid-point-circle-drawing-algorithm/
	List<HashSet<Vector2>> GenerateMidPointCircle(int radius)
	{
		List<HashSet<Vector2>> points = new List<HashSet<Vector2>>();

		for(int radiusIter = 0; radiusIter <= radius; radiusIter++)
		{
			//create list
			points.Add(new HashSet<Vector2>());

			int x = radiusIter;
			int y = 0;

			//center is 0,0

			//x + x_center, y + y_center
			points[radiusIter].Add(new Vector2(x,y));

			//radius 1 -> 1+
			if(radiusIter > 0)
			{
				//Changes these from the example drawing alg because they didn't give the points wanted.
				//Not sure if its my mistake or theirs but these values give me an adjacency overlay that looks like what I expected
				points[radiusIter].Add(new Vector2(-x,y));
				points[radiusIter].Add(new Vector2(y,x));
				points[radiusIter].Add(new Vector2(y,-x));
				
			}
			
			//Midpoint of two pixels
			int midpoint = 1 - radiusIter;
			while (x > y)
			{
				++y;

				//Midpoint inside or on perimeter
				if(midpoint <= 0)
				{
					midpoint += (2 * y) + 1;
				}
				else	//Outside perimeter
				{
					--x;
					midpoint += + (2 * y) - (2 * x) + 1;
				}

				if(x < y)
				{
					//break this perimeter iter
					break;
				}

				//Print each octant of the circle (4 points on each of the quadrents as this func iterates a single quadrent
				//x + x_center, y + y_center
				points[radiusIter].Add(new Vector2(x,y));
				points[radiusIter].Add(new Vector2(-x,y));
				points[radiusIter].Add(new Vector2(x,-y));
				points[radiusIter].Add(new Vector2(-x,-y));

				if(x != y)
				{
					points[radiusIter].Add(new Vector2(y,x));
					points[radiusIter].Add(new Vector2(-y,x));
					points[radiusIter].Add(new Vector2(y,-x));
					points[radiusIter].Add(new Vector2(-y,-x));
				}
			}
		}

		return points;
	}

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
		MapGenColorListNode = GetTree().Root.FindNode("MapGenColorList/VBoxContainer2");

		ActiveOverlayOptions = GetNode("Camera2D/GUI/VBoxContainer/SelectedOverlay") as OptionButton;
		
		//link forground and background map variables to the nodes

		random = new Random();

		//fill neighbors offset for any arbitrary vector, precalced into a container
		neighborsToCheck = GenerateMidPointCircle(6);


		neighborsToCheckSingle = new Vector2[8];

		int pos = 0;
		for(int i = -1; i <= 1; ++i)  
		{
			for(int j = -1; j <= 1; ++j)  
			{
				if(i == 0 && j == 0)
					continue;
				neighborsToCheckSingle[pos++] = new Vector2(i, j);
			}
		}
		VisualizationMaps = new Dictionary<string, TileMap>();
		ForegroundMap = GetNode("ForegroundMap") as TileMap;

		//List visualization maps here
		VisualizationMaps["Adjacency Overlay"] = GetNode("AdjacencyMap") as TileMap;
		VisualizationMaps["Another Overlay"] = GetNode("AnotherOverlayMap") as TileMap;

		//Generate the options menu from the dict keys to make sure they are good with 0 still being no overlays
		foreach (var item in VisualizationMaps)
		{
			ActiveOverlayOptions.AddItem(item.Key);
		}
		
		Generate_CCL_Select_Largest_Adj();

		//Set the CCLGen Adjacency Overlay map to output adjacency data to
		CCLGen.SetVisualizationMap(ref VisualizationMaps, "Adjacency Overlay");
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
	public void UpdateMapData()
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
				foreach (Vector2 tilePos in neighborsToCheckSingle)
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

	//Maybe try to grow a rectangle into 3x3 or larger and classify that as a "room" 
	//each coord has the distnace to the closest wall, higher points are more open areas aka larger rooms?
	//Turn every point into its own Square (Corner to center navigable) and then attempt to grow each square ring
	//Also displays it green, yellow, orange, red, purple, blue, cyan for 1,2,3,4,5,6,7
	//Runs on the terrainMap to find the closest number of tiles
	private void GenerateAdjacencyGrid()
	{
		closestWalls.Clear();

		foreach (var largestItem in largestSet)
		{
			//3x3,5x5,7x7,9x9 +-1 on x,y +-2 on x,y +-3 on xy until wall
			//For growing square borders size 1,9,25,49 and if they contain walls then thats the distance
			int squareSize = 0;
			bool foundWall = false;
			//can use neighborsToCheckAdj or neighborsToCheckDiag
			foreach (var set in neighborsToCheck)
			{
				foreach (var item in set)
				{
					//if wall
					int posX = largestItem.Key + (int)item.x;
					int posY = largestItem.Value + (int)item.y;

					//Make sure coordinates are inside of the terrain
					posX = Mathf.Max(0,Mathf.Min(width - 1, posX));
					posY = Mathf.Max(0,Mathf.Min(height - 1, posY));
					
					if(terrainMap[posX,posY] == 1)
					{ 
						foundWall = true;
						closestWalls[new KeyValuePair<int, int>(largestItem.Key,largestItem.Value)] = squareSize;
						break;
					}
				}
				squareSize++;
				if(foundWall)
					break;
			}
		}

		for(int x = 0; x < width; ++x)
		{
			for(int y = 0; y < height; ++y)
			{
				//Set cell to -1 deletes it
				VisualizationMaps["Adjacency Overlay"].SetCell(-x + width / 2, -y + height / 2, -1);
			}
		}

		foreach (var item in largestSet)
		{
			VisualizationMaps["Adjacency Overlay"].SetCell(-item.Key + width / 2, -item.Value + height / 2, (closestWalls[item] * 4) % maxColors);
		}
	}
}
