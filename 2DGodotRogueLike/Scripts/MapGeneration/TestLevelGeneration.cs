using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class TestLevelGeneration : Node2D
{

#region Signals

  public void FloodFillToDirectedGraph_Callback()
  {
    CPF.GenerateDirectedGraphFromFloodFill(out cpfRootNode, new Vector2(largestSet[0].Key, largestSet[0].Value), null, false, true);
  }

  public void FloodFillToDirectedGraphIter_Callback()
  {
    for (int i = 0; i < numDirectedGraphFromFloodFillIter; i++)
    {
      CPF.GenerateDirectedGraphFromFloodFill(out cpfRootNode, new Vector2(largestSet[0].Key, largestSet[0].Value), null, false, true);
    }
  }

  public void NumFloodFillDebugIter(float val)
  {
    numDirectedGraphFromFloodFillIter = (int)val;
    //https://www.reddit.com/r/godot/comments/jcqj6f/how_to_release_focus_out_of_a_spin_box_after/
    //Wow this is irritating that these both need to be here to kick the keyboard out after entering a number
    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer/SpinBox") as SpinBox).ReleaseFocus();
    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer/SpinBox") as SpinBox).GetLineEdit().ReleaseFocus();
  }

  public void SetNumKMeansClusters(float val)
  {
    numKMeansClusters = (int)val;

    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer2/HSplitContainer/SpinBox2") as SpinBox).ReleaseFocus();
    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer2/HSplitContainer/SpinBox2") as SpinBox).GetLineEdit().ReleaseFocus();
  }

  public void SetIterKMeansClusters(float val)
  {
    iterKMeans = (int)val;

    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer2/HSplitContainer/SpinBox3") as SpinBox).ReleaseFocus();
    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer2/HSplitContainer/SpinBox3") as SpinBox).GetLineEdit().ReleaseFocus();
  }

  public void SetGameOfLifeDeadChance(float val)
  {
    initialDeadChance = (int)val;

    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer3/SpinBox") as SpinBox).ReleaseFocus();
    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer3/SpinBox") as SpinBox).GetLineEdit().ReleaseFocus();
  }

  public void RunIterPerFrame()
  {
    realtimeFloodFill = true;
    CPF.ResetFloodFill();
  }

  public void FloodFillToDirectedGraphReset_Callback()
  {
    realtimeFloodFill = false;
    CPF.ResetFloodFill();
  }

  public void DirectedGraphToTarjans_Callback()
  {
    
  }

  public void TarjansToRooms_Callback()
  {

  }

  public void SpawnOreChunksRoom_pressed()
  {
    foreach (var room in rooms)
    {
      foreach (var point in room)
      {
        if(point.Value > 2)
        {
          if(random.NextDouble() < oreChunkRoomSpawnChance)
          {
            SpawnOreChunkAt(new Vector2(point.Key, point.Value));
          }
        }
      }
    }
  }

  public void SpawnOreChunksEdge_pressed()
  {
    foreach (var item in adjacencyMap)
    {
      if(item.Value == 1)
      {
        if(random.NextDouble() < oreChunkEdgeSpawnChance)
        {
          SpawnOreChunkAt(new Vector2(item.Key.Key , item.Key.Value));
        }
      }
    }
  }

  public void GenerateRoomsFromFloodFillAndAdjacency()
  {
    //Select every point in the adjacency man that has a adjacency value >= 2 and returns those points into a IEnumerable
    //
    //List<KeyValuePair<int,int>> points = CPF.FloodFill((x,y)=>{return terrainMap[x,y] == 0 && adjacencyMap[new KeyValuePair<int,int>(x,y)] >= 2;}, true, false)

    HashSet<KeyValuePair<int,int>> checkedPts = new HashSet<KeyValuePair<int, int>>();
    //For each value >=2 
    rooms = adjacencyMap.Where(point => point.Value > mapRoomMinDist).Select(pt => pt.Key).ToList()  //Creates list of points.Value >= 2
      //flood fill each point
      .Select(p => CPF.FloodFill(checkedPts,p,(x,y)=>{return terrainMap[x,y] == 0 && adjacencyMap[new KeyValuePair<int,int>(x,y)] >= mapRoomMinDist && !checkedPts.Contains(new KeyValuePair<int,int>(x,y));}, true)).Where(l => l.Count > 0).ToList(); //Generates list of list of flood fill points
      //Make sets of point clouds unique
      //.Distinct(); //Selects unique lists

    VisualizationMaps["Room Overlay"].Clear();
    int currListValue = 0;
    foreach (var pointList in rooms)
    {
      foreach (var item in pointList)
      {
        VisualizationMaps["Room Overlay"].SetCell(item.Key, item.Value, currListValue % maxColors);
      }
      currListValue++;
    }

    //Weighted KMeans, doesnt quite give the result as wanted
    //=================================================================================
    //var seedPoints = points.Select(pointArray => pointArray[0]).ToList(); //Selects the first value of each list and turns that into its own list
    //
    //Dictionary<KeyValuePair<int,int>, float> seedPointsToWeight = new Dictionary<KeyValuePair<int, int>, float>();
    //
    //for (int i = 0; i < seedPoints.Count; i++)
    //{
    //  seedPointsToWeight.Add(seedPoints[i], Mathf.Sqrt(points[i].Count));  
    //}
    //
    //CPF.GenerateWeightedKMeansFromTerrain(seedPoints.Count, largestSet, seedPointsToWeight, iterKMeans);
    //End Weighted KMeans
    //=================================================================================
  }

  //Translates grid vec to center of vec in world pos
  public Vector2 MapPointToWorldPos(Vector2 pos)
  {
    return pos * ForegroundMap.CellSize * ForegroundMap.Scale + (ForegroundMap.CellSize * ForegroundMap.Scale * 0.5f);
  }

  public void SpawnPlayerAtStartRoom()
  {
    if(startRoom != -1)
    {
      PlayerTopDown newPlayer = playerObjectScene.Instance() as PlayerTopDown;
      
      GetTree().Root.AddChild(newPlayer);
      playerCamera = newPlayer.GetNode("Camera2D") as Camera2D;
      newPlayer.GlobalPosition = MapPointToWorldPos(new Vector2(rooms[startRoom][0].Key, rooms[startRoom][0].Value));
      SetPlayerMode(true);
      playerManager.SetTopDownPlayer(ref newPlayer);
    }
    else
    {
      Console.WriteLine("Start Room not set");
    }
  }

  public void SetPlayerMode(bool _playerMode)
  {
    playerMode = _playerMode;
    if(playerMode)
    {
      debugCamera.Current = false;
      (debugCamera as CameraMovement).movementEnabled = false;
      DebugUI.Visible = false;
      if(playerCamera != null)
        playerCamera.Current = true;
    }
    else
    {
      (debugCamera as CameraMovement).movementEnabled = true;
      debugCamera.Current = true;
      DebugUI.Visible = true;
      if(playerCamera != null)
        playerCamera.Current = false;
    }
  }

  //Calculates the distances from each room to each other room
  public void FindFarthestRooms()
  {
    //index of room as X/Y start to end
    roomDistances = new int[rooms.Count,rooms.Count];
    int currStartRoom = 0;
    
    //potentially every room can be an end room
    potentialEndRooms = new List<int>();
    
    foreach (var startRoom in rooms)
    {
      int currEndRoom = 0;
      foreach (var endRoom in rooms)
      {
        if(currStartRoom != currEndRoom && roomDistances[currStartRoom, currEndRoom] == 0)
        {
          //Need new map
          AStarMap = new AStar.AStarMap(terrainMap, width, height);   
          AStarPather.InitPather(new Vector2(startRoom[0].Key, startRoom[0].Value),new Vector2(endRoom[0].Key, endRoom[0].Value), AStarMap);
          var result = AStarPather.GeneratePath();  
          
          roomDistances[currStartRoom, currEndRoom] = AStarPather.path.Count();
          //Update each other start/end pair
          roomDistances[currEndRoom, currStartRoom] = roomDistances[currStartRoom, currEndRoom];

          if(result == AStar.PathState.DoesNotExist)
          {
            Console.WriteLine("Path Does not exists between " + startRoom[0].ToString() + " and " + endRoom[0].ToString());
          }
          else if (result == AStar.PathState.Found)
          {
            Console.WriteLine("Path Found between " + startRoom[0].ToString() + " and " + endRoom[0].ToString());
          }
        }
        currEndRoom++;
      }
      currStartRoom++;
    }

    int longestPath = 0;
    int longestStartRoom = 0;
    int longestEndRoom = 0;

    for (int i = 0; i < rooms.Count; i++)
    {
      //Only check along diagonal
      for (int j = i; j < rooms.Count; j++)
      {
        if(roomDistances[i,j] > longestPath)
        {
          longestStartRoom = i;
          longestEndRoom = j;
          longestPath = roomDistances[i,j];
        }

      }
    }

    for (int i = 0; i < rooms.Count; i++)
    {
      //Add all possible rooms with an exit to a list so that we can create many exits if we want to
      if(roomDistances[longestStartRoom,i] > minDistToEndRoom)
      {
        potentialEndRooms.Add(i);
      }
    }

    Console.WriteLine("Farthest apart rooms is Room " + longestStartRoom.ToString() + " and Room " + longestEndRoom.ToString());

    //Clear AStar vis map
    VisualizationMaps["AStar Overlay"].Clear();

    //Draw rooms that are far enough 
    foreach (var item in potentialEndRooms)
    {
      VisualizationMaps["AStar Overlay"].SetCell(rooms[item][0].Key,rooms[item][0].Value, 10);
    }

    //Draw start and abs farthest
    VisualizationMaps["AStar Overlay"].SetCell(rooms[longestStartRoom][0].Key,rooms[longestStartRoom][0].Value, 4);
    VisualizationMaps["AStar Overlay"].SetCell(rooms[longestEndRoom][0].Key,rooms[longestEndRoom][0].Value, 11);

    startRoom = longestStartRoom;
    endRoom = longestEndRoom;
  }

  public void SpawnOreChunkAt(Vector2 pos )
  {
    OreWorldObject newObj = oreWorldObjectScene.Instance() as OreWorldObject;

    //Spawn ore chunk at item.Key * scale + offset (0,0 is upper left corner) so they are centered
    newObj.Position = pos * ForegroundMap.CellSize * ForegroundMap.Scale + (ForegroundMap.CellSize * ForegroundMap.Scale * 0.5f);
    newObj.material = (Materials.Material)random.Next(0,6);
    newObj.amountInOre = random.Next(1,6);
    newObj.timeToMine = random.Next(2,4);

    //Calls _Ready func
    GetTree().Root.AddChild(newObj);

    if(newObj.material == Materials.Material.Copper)
      newObj.animatedSprite.Animation = "Copper Ores";
    if(newObj.material == Materials.Material.Cobalt)
      newObj.animatedSprite.Animation = "Cobalt Ores";

    newObj.animatedSprite.Frame = random.Next(0,10);
    newObj.ZIndex = -1;
  }


  //General Signals
  public void GenerateNewTileMapButton_Callback()
  {
    GD.Print("Clicked Generate New Tile Map Button");
    GenerateMap(maxIterations, true);
    UpdateInternalMaps();
    UpdateForegroundMapData();
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
    UpdateInternalMaps();
    CCLGen.CCLAlgorithm();
    largestSet = CCLGen.GetLargestSet();
    UpdateForegroundMapData();
    ClearSmallerCaves();
  }

  public void CCL_IterateSimulationOnce_Callback()
  {
    GD.Print("Clicked Prune Tile Map Button");
    GenerateMap(1, false);
    UpdateInternalMaps();
    UpdateForegroundMapData();
  }

  public void CCL_GenerateCaveGroups_Callback()
  {
    GD.Print("Generate Cave Groups Button");
    CCLGen.CCLAlgorithm();
    UpdateForegroundMapData();
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
    UpdateInternalMaps();
    WFCSTM.UpdateInternalMap(width, height, ref terrainMap);
    //WFCSTM.CCLAlgorithm();
    UpdateForegroundMapData();
  }

  public void WFC_IterateSimulationOnce_Callback()
  {
    GD.Print("Clicked Prune Tile Map Button");
    GenerateMap(1, false);
    UpdateInternalMaps();
    WFCSTM.UpdateInternalMap(width, height, ref terrainMap);
    UpdateForegroundMapData();
  }

  public void GenAll()
  {
    //Clear old
    CCLGen.Clear();
    CPF.Clear();
    //WFCSTM.Clear();
    
    //Generate Map
    GenerateMap(maxIterations, true);
    if(mapFinalScale > 1)
    {
      UpscaleMap(mapFinalScale);  //3 so the player can go through stuff
      GenerateMap(1, false);
    }
    
    UpdateInternalMaps();
    UpdateForegroundMapData();


    //CCL
    CCLGen.CCLAlgorithm();
    
    //Get Largest Set
    largestSet = CCLGen.GetLargestSet();
    ClearSmallerCaves();

    //Adjacency Vis Map
    GenerateAdjacencyGrid();
    
    //Directed Graph Vis Map
    FloodFillToDirectedGraph_Callback();

    //KMeans Vis Map
    RunKMeansOnLargestSet();

    //Room finder
    GenerateRoomsFromFloodFillAndAdjacency();
  }

  //Generated everything, finds farthest rooms, spawns chunks, spawns player
  public void GenAllAndSpawnOreAndPlayer()
  {
    GenAll();
    FindFarthestRooms();
    SpawnOreChunksEdge_pressed();
    SpawnPlayerAtStartRoom();
  }

  public void Generate_CCL_Select_Largest_Adj()
  {
    GenerateMap(maxIterations, true);
    UpdateInternalMaps();
    CCLGen.CCLAlgorithm();
    largestSet = CCLGen.GetLargestSet();
    UpdateForegroundMapData();
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

  public void MouseOptionsSelected_Callback(int index)
  {
    currentMouseOption = (MouseOptions)index;
  }

  public void RunKMeansOnLargestSet()
  {
    CPF.GenerateKMeansFromTerrain(numKMeansClusters, largestSet, null, iterKMeans);
  }

  public void ToggleRealtimeAStar(bool toggle)
  {
    realtimeAStar = toggle;
  }

  public void SetRealtimeAStarIter(float iters)
  {
    realtimeAStarIter = (int)iters;

    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer4/SpinBox3") as SpinBox).ReleaseFocus();
    (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer4/SpinBox3") as SpinBox).GetLineEdit().ReleaseFocus();
  }

#endregion

#region Variables
  public CCLGenerator CCLGen = new CCLGenerator();
  public WFCSimpleTiledModel WFCSTM = new WFCSimpleTiledModel();
  public ChokePointFinder CPF = new ChokePointFinder();

  List<KeyValuePair<int, int>> largestSet = new List<KeyValuePair<int, int>>();

  public const int maxColors = 47;

  public PackedScene IDColorMapScene = ResourceLoader.Load<PackedScene>("res://TemplateScenes/IDAndColorUIElement.tscn");
  Node MapGenColorListNode;

  ChokePointFinder.CPFNode cpfRootNode = new ChokePointFinder.CPFNode();

  //init with terrain map
  AStar.AStarMap AStarMap;
  AStar.AStarPather AStarPather = new AStar.AStarPather();
  //Flood Fill Vars
  int numDirectedGraphFromFloodFillIter = 1;
  bool realtimeFloodFill = false;

  //KMeans Vars
  int numKMeansClusters = 10;
  int iterKMeans = 100;

  //AStar Vars
  bool realtimeAStar = false;
  int realtimeAStarIter = 1;

  bool setAStarStart = false;
  bool setAStarDest = false;
  AStar.PathState AStarState = AStar.PathState.None;
  Vector2 AStarStart;
  Vector2 AStarDest;

  MouseOptions currentMouseOption = MouseOptions.AStar_Pathfind;

  Label currentMouseSelectionAStar_NodeCoords;
  Label currentMouseSelectionAStar_GivenCost;
  Label currentMouseSelectionAStar_Heuristic;
  Label currentMouseSelectionAStar_ParentNodeCoords;
  Label currentMouseSelectionAStar_NodeState;
  AStar.AStarNode currentMouseSelectionNode = new AStar.AStarNode();
  MarginContainer DebugUI;

  //0 to 1
  float oreChunkEdgeSpawnChance = 0.1f;

  //0 to 1
  float oreChunkRoomSpawnChance = 0.2f;

  private PackedScene oreWorldObjectScene = (PackedScene)ResourceLoader.Load("res://TemplateScenes/OreWorldObject.tscn");
  private PackedScene playerObjectScene = (PackedScene)ResourceLoader.Load("res://TemplateScenes/TopDownPlayerScene.tscn");

  List<List<KeyValuePair<int,int>>> rooms = new List<List<KeyValuePair<int, int>>>();

  //Array of room distances to each other
  int[,] roomDistances;

  //Index of starting room + ending room
  int endRoom = -1;
  int startRoom = -1;
  List<int> potentialEndRooms;
  
  //num nodes not full distance atm, slightly faster to do it like this than dist calcs even though not as accurate
  int minDistToEndRoom = 50;

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

  public enum MouseOptions
  {
    AStar_Pathfind,
    AStar_NodeInfo,
    DirectedGraph_ToParent,
  };

  public Dictionary<MouseOptions,string> mouseOptionsDir = new Dictionary<MouseOptions, string>();

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
    Colors.Green,   //0
    Colors.Yellow,  //1
    Colors.Orange,  //2
    Colors.Red,     //3
    Colors.Purple,  //4
    Colors.Blue,     //5
    Colors.Cyan,     //6
    Colors.White};  //7

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
  Dictionary<KeyValuePair<int,int>, int> adjacencyMap = new Dictionary<KeyValuePair<int, int>, int>();

  OptionButton ActiveOverlayOptions;
  OptionButton MouseOptionsButton;


  Camera2D debugCamera;
  Camera2D playerCamera;

  InputManager inputManager;
  PlayerManager playerManager;

  bool playerMode = false;

  //Final scale of the map, upscales initial caves by this amt
  int mapFinalScale = 1;
  int mapRoomMinDist = 2; 
  //mapFinalScale of 1 should have a mapRoomMinDist of 2
  //mapFinalScale of 2 should have a mapRoomMinDist of 5

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
      //  maxValues.x = coord.Key;
      //}
      //if(coord.Value > maxValues.y)
      //{
      //  maxValues.y = coord.Value;
      //}
    }

    //This is incase we want the max bounds of the cave to clear away extra floor tiles
    //for(int y = 0; y < height; ++y)
    //{
    //  for(int x = 0; x < width; ++x)
    //  {  
    //    if(x > maxValues.x + 1)
    //    {
    //
    //    }
    //    terrainMap[x,y] = 1;
    //  }
    //}
    UpdateForegroundMapData();
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
        else  //Outside perimeter
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

  bool FindNodeAtPos(Vector2 pos, ChokePointFinder.CPFNode node, out ChokePointFinder.CPFNode foundNode)
  {
    
    if(node.pos == pos.Round())
    {
      foundNode = node;
      return true;
    }

    foreach (var child in node.children)
    {
      if(FindNodeAtPos(pos, child, out foundNode))
      {
        return true;
      }
    }
    foundNode = null;
    return false;
  }
  
  //Fixed
  public override void _PhysicsProcess(float delta)
  {
    if(realtimeFloodFill == true)
    {
      for (int i = 0; i < numDirectedGraphFromFloodFillIter; i++)
      {
        if(CPF.GenerateDirectedGraphFromFloodFill(
          out cpfRootNode, 
          new Vector2(largestSet[largestSet.Count/2].Key, 
          largestSet[largestSet.Count/2].Value), null, false, true))
        {
          realtimeFloodFill = false;
          //Set the button to display as not toggled
          (GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer/HSplitContainer/WFC_ViewSockets6") as Button).ToggleMode = false;
          break;
        }
      }
    }

    if(realtimeAStar == true && AStarState == AStar.PathState.Searching)
    {
      for (int i = 0; i < realtimeAStarIter; i++)
      {
        AStarState = AStarPather.GeneratePath(realtimeAStar);
        if(AStarState == AStar.PathState.Found)
          break;
      }
      AStarPather.UpdateMapVisual();
    }
  }

  //Draw parent tree with simple parent follow set to the tile to show the path to the parent
  void DrawParentTree(ChokePointFinder.CPFNode currNode)
  {
    VisualizationMaps["Directed Graph Overlay"].SetCell((int)currNode.pos.x , (int)currNode.pos.y , 10);
    if(currNode.parent == null)
    {
      return;
    }
    else
    {
      DrawParentTree(currNode.parent);
    }
  }

  void UpdateMouseInfoUI()
  {
    //https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
    //5 points after decimal
    currentMouseSelectionAStar_NodeCoords.Text = currentMouseSelectionNode.pos.ToString("F5");
    currentMouseSelectionAStar_GivenCost.Text = currentMouseSelectionNode.givenCost.ToString("F5");
    currentMouseSelectionAStar_Heuristic.Text = currentMouseSelectionNode.heuristic.ToString("F5");
    if(currentMouseSelectionNode.parent != null)  
      currentMouseSelectionAStar_ParentNodeCoords.Text = currentMouseSelectionNode.parent.pos.ToString("F5");
    currentMouseSelectionAStar_NodeState.Text = currentMouseSelectionNode.state.ToString();
  }

  //Select with mouse position
  public override void _UnhandledInput(InputEvent inputEvent)
  {
    if(currentMouseOption == MouseOptions.DirectedGraph_ToParent)
    {
      if (@inputEvent is InputEventMouseButton mouseClick && (mouseClick.Pressed && mouseClick.ButtonIndex == (int)Godot.ButtonList.Left))
      {
        //World space -> Map space where coordinates are
        Vector2 clickedPos = ForegroundMap.WorldToMap(ForegroundMap.GetLocalMousePosition());
        ChokePointFinder.CPFNode foundNode;
        if(FindNodeAtPos(clickedPos, cpfRootNode, out foundNode))
        {
          //Reset the visuals
          CPF.UpdateDirectedMapVis(cpfRootNode);
          DrawParentTree(foundNode);
          //Console.WriteLine("Found Node! at " + clickedPos.ToString() + " Where node exists at " + foundNode.pos.ToString());
        }
      }
    }
    else if (currentMouseOption == MouseOptions.AStar_Pathfind)
    {
      if(@inputEvent is InputEventMouseButton mouseClick && mouseClick.Pressed)
      {
        if (mouseClick.ButtonIndex == (int)Godot.ButtonList.Right)
        {
          AStarState = AStar.PathState.None;
          Vector2 newAStarDest = ForegroundMap.WorldToMap(ForegroundMap.GetLocalMousePosition());

          if(newAStarDest.x >= width || newAStarDest.x <= 0)
            return;
          if(newAStarDest.y >= height || newAStarDest.y <= 0)
            return;
            
          //Dont allow walls to be set as dest or start
          if(terrainMap[(int)newAStarDest.x,(int)newAStarDest.y] == 0)
          {
            //Remove old one
            VisualizationMaps["AStar Overlay"].SetCell((int)AStarDest.x,(int)AStarDest.y, -1);

            AStarDest = newAStarDest;
            //Set dest
            setAStarDest = true;
            VisualizationMaps["AStar Overlay"].SetCell((int)AStarDest.x,(int)AStarDest.y, 11);
          }
          else
          {
            AStarDest = Vector2.Zero;
            setAStarDest = false;
          }
        }

        if (mouseClick.ButtonIndex == (int)Godot.ButtonList.Left)
        {
          AStarState = AStar.PathState.None;
          Vector2 newAStarStart = ForegroundMap.WorldToMap(ForegroundMap.GetLocalMousePosition());

          if(newAStarStart.x >= width || newAStarStart.x <= 0)
            return;
          if(newAStarStart.y >= height || newAStarStart.y <= 0)
            return;
            
          //Dont allow walls to be set as dest or start
          if(terrainMap[(int)newAStarStart.x,(int)newAStarStart.y] == 0)
          {
            VisualizationMaps["AStar Overlay"].SetCell((int)AStarStart.x,(int)AStarStart.y, -1);

            AStarStart = newAStarStart;
            //Set start
            setAStarStart = true;
            VisualizationMaps["AStar Overlay"].SetCell((int)AStarStart.x,(int)AStarStart.y, 4);
          }
          else
          {
            AStarStart = Vector2.Zero;
            setAStarStart = false;
          }
        }
      }
      if(setAStarStart && setAStarDest && AStarState == AStar.PathState.None)
      {
        AStarMap = new AStar.AStarMap(terrainMap, width, height);
        //World space -> Map space where coordinates are
        AStarPather.InitPather(AStarStart, AStarDest, AStarMap);
        //if not realtime then just generate the path
        
        if(!realtimeAStar)
        {
          AStarState = AStarPather.GeneratePath(realtimeAStar);
          AStarPather.UpdateMapVisual();
        }
        else
        {
          AStarState = AStar.PathState.Searching;
        }
      }
    }
    else if (currentMouseOption == MouseOptions.AStar_NodeInfo)
    {
      if(@inputEvent is InputEventMouseButton mouseClick && mouseClick.Pressed && mouseClick.ButtonIndex == (int)Godot.ButtonList.Left)
      {
        if(AStarPather.map != null)
        {
          currentMouseSelectionNode = AStarPather.map.GetNodeAt(ForegroundMap.WorldToMap(ForegroundMap.GetLocalMousePosition()));
        }

        UpdateMouseInfoUI();
      }
    }

    if(inputManager.IsKeyPressed(KeyList.M))
    {
      SetPlayerMode(!playerMode);
    }
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
        
    inputManager = GetNode<InputManager>("/root/InputManagerSingletonNode");
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");

    debugCamera = GetNode("Camera2D") as Camera2D;

    MapGenColorListNode = GetTree().Root.FindNode("MapGenColorList/VBoxContainer2");

    ActiveOverlayOptions = GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer5/SelectedOverlay") as OptionButton;

    MouseOptionsButton = GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer6/MouseOptions") as OptionButton;

    currentMouseSelectionAStar_NodeCoords = GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General7") as Label;
    currentMouseSelectionAStar_GivenCost = GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General8") as Label;
    currentMouseSelectionAStar_Heuristic = GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General9") as Label;
    currentMouseSelectionAStar_ParentNodeCoords = GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General10") as Label;
    currentMouseSelectionAStar_NodeState = GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General11") as Label;

    DebugUI = GetNode("Camera2D/GUI") as MarginContainer;

    random = new Random();

    //fill neighbors offset for any arbitrary vector, precalced into a container
    neighborsToCheck = GenerateMidPointCircle(50);

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
    VisualizationMaps["Directed Graph Overlay"] = GetNode("Directed Graph Overlay") as TileMap;
    VisualizationMaps["Room Overlay"] = GetNode("Room Overlay") as TileMap;
    VisualizationMaps["KMeans Overlay"] = GetNode("KMeans Overlay") as TileMap;
    VisualizationMaps["AStar Overlay"] = GetNode("AStar Overlay") as TileMap;
    

    //Generate the options menu from the dict keys to make sure they are good with 0 still being no overlays
    foreach (var item in VisualizationMaps)
    {
      ActiveOverlayOptions.AddItem(item.Key);
    }

    //Generate the options menu from the dict keys to make sure they are good with 0 still being no overlays
    foreach (var item in Enum.GetNames(typeof(MouseOptions)))
    {
      MouseOptionsButton.AddItem(item);
    }

    //Set the CCLGen Adjacency Overlay map to output adjacency data to
    CCLGen.SetVisualizationMap(ref VisualizationMaps, "Adjacency Overlay");
    CPF.SetDirectedGraphVisualizationMap(ref VisualizationMaps, "Directed Graph Overlay");
    CPF.SetRoomVisualizationMap(ref VisualizationMaps, "Room Overlay");
    CPF.SetKMeansVisMap(ref VisualizationMaps, "KMeans Overlay");
    AStarPather.SetAStarVisualizationMap(ref VisualizationMaps, "AStar Overlay");
    
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
    //TODO VVV
    CPF.Clear();
    terrainMap = null;
  }

#region Game of Life

  //for each tile places scaleMultiplier tiles for each current tile
  private void UpscaleMap(int scaleMultiplier)
  {
    int [,] newTerrainMap = new int[width*scaleMultiplier, height*scaleMultiplier];
    
    for (int i = 0; i < width; i++)
    {
      for (int j = 0; j < height; j++)
      {
        //X increase
        for (int k = 0; k < scaleMultiplier; k++)
        {
          //Y increase
          for (int l = 0; l < scaleMultiplier; l++)
          {
            newTerrainMap[i*scaleMultiplier+k, j*scaleMultiplier+l] = terrainMap[i,j];
          }
        }
      }
    }

    //Update current stuff
    width = width*scaleMultiplier;
    height = height*scaleMultiplier;
    terrainMap = newTerrainMap; 
  }

  //Creates a new map of the tileMapSize and iterates the game of life a set number of times
  private void GenerateMap(int iterations, bool newMap)
  {

    if(newMap)
    {
      width = (int) tileMapSize.x;
      height = (int) tileMapSize.y;
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
    //I think its wall now...
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

    bool floorExists = false;

    //Check if every single spot is a wall
    for(int y = 0; y < height; ++y)
    {
      if(floorExists)
        break;
      for(int x = 0; x < width; ++x)
      {  
        if(terrainMap[x,y] != 1)
        {
          floorExists = true;
          break;
        }
      }
    }
    
    //just regen if no map
    if(!floorExists)
    {
      GenerateMap(iterations, newMap);
    }
  }

  private void UpdateInternalMaps()
  {
    CCLGen.UpdateInternalMap(width, height, ref terrainMap);
    CPF.UpdateInternalMap(width, height, ref terrainMap);
    WFCSTM.UpdateInternalMap(width, height, ref terrainMap);
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
  public void UpdateForegroundMapData()
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
          ForegroundMap.SetCell(x , y , TopTile);
        }
        else  //Bottom Tile = 0 = Grass Wall
        {
          ForegroundMap.SetCell(x , y , BottomTile);

        }

        //set the background to all bottom tile????
        //BackgroundMap.SetCell(-x , -y , BottomTile);
      }
    }

    //ForegroundMap.SetCell(-width / 2, -height / 2, 0);
    //oregroundMap.SetCell(0, 0, 1);
    //ForegroundMap.SetCell( width / 2, -(width - 1)  , 0);


    //update bitmask for auto tile
    ForegroundMap.UpdateBitmaskRegion(new Vector2(0, 0), new Vector2(width, height));

    ForegroundMap.UpdateDirtyQuadrants();
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

#endregion
  //Maybe try to grow a rectangle into 3x3 or larger and classify that as a "room" 
  //each coord has the distance to the closest wall, higher points are more open areas aka larger rooms?
  //Turn every point into its own Square (Corner to center navigable) and then attempt to grow each square ring
  //Also displays it green, yellow, orange, red, purple, blue, cyan for 1,2,3,4,5,6,7
  //Runs on the terrainMap to find the closest number of tiles
  private void GenerateAdjacencyGrid()
  {
    adjacencyMap.Clear();

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
            adjacencyMap[new KeyValuePair<int, int>(largestItem.Key,largestItem.Value)] = squareSize;
            break;
          }
        }
        squareSize++;
        if(foundWall)
          break;
      }
      if(foundWall == false)
      {
        Console.WriteLine("Did not find wall in radius 50");
        adjacencyMap[new KeyValuePair<int, int>(largestItem.Key,largestItem.Value)] = int.MaxValue;
      }
    }

    //for(int x = 0; x < width; ++x)
    //{
    //  for(int y = 0; y < height; ++y)
    //  {
    //    //Set cell to -1 deletes it
    //    VisualizationMaps["Adjacency Overlay"].SetCell(x , y , -1);
    //  }
    //}
    VisualizationMaps["Adjacency Overlay"].Clear();

    foreach (var item in largestSet)
    {
      VisualizationMaps["Adjacency Overlay"].SetCell(item.Key , item.Value , (adjacencyMap[item] * 3) % maxColors);
    }
  }
}
