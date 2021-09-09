using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ChokePointFinder
{
  // Flood Fill (sorted via dist^2 calc) -> Directed Graph
  // Trajan's Algorithm on Directed Graph -> Choke Points
  // Choke Points -> Flood Fill until Pts
  // Creates rooms
#region Subclass Definitions
  public enum NodeState
  {
    NotChecked,
    Closed
  }
  
  public class CPFNode
  {
    public Vector2 pos = new Vector2();
    public CPFNode parent;
    public List<CPFNode> children = new List<CPFNode>();
    //public NodeState state = NodeState.NotChecked;
  };

  public class NodeQueueSorter : Comparer<CPFNode>
  {
    CPFNode rootNode;
    public NodeQueueSorter(CPFNode _rootNode)
    {
      rootNode = _rootNode;
    }
    public override int Compare(CPFNode x, CPFNode y)
    {
      return x.pos.DistanceSquaredTo(rootNode.pos).CompareTo(y.pos.DistanceSquaredTo(rootNode.pos));
    }
  }

#endregion

#region Variables
  public const int maxColors = 112;
  int [,] terrainMap;
	int width;
  int height;

  Godot.TileMap directedGraphVisMap = new TileMap();
  Godot.TileMap roomVisMap = new TileMap();
  CPFNode rootNode = new CPFNode();
  Dictionary<Vector2, int> DebugDirToTile = new Dictionary<Vector2, int>
  {
    //Y down
    {new Vector2(0,0), 4},
    {new Vector2(0,1), 3},
    {new Vector2(0,-1), 5},
    {new Vector2(1,0), 1},
    {new Vector2(-1,0), 7},
    {new Vector2(1,1), 8},
    {new Vector2(-1,1), 2},
    {new Vector2(1,-1), 6},
    {new Vector2(-1,-1), 0},
  };

#endregion
#region Map Funcs
  public void SetDirectedGraphVisualizationMap(ref Dictionary<string,Godot.TileMap> _mapIDVisualization, string map)
	{
		directedGraphVisMap = _mapIDVisualization[map];
	}

  public void SetRoomVisualizationMap(ref Dictionary<string,Godot.TileMap> _mapIDVisualization, string map)
	{
		roomVisMap = _mapIDVisualization[map];
	}

	public void UpdateInternalMap(int _width, int _height, ref int [,] _terrainMap)
	{
    //Delete children
    rootNode = new CPFNode();
		width = _width;
		height = _height;
		terrainMap = _terrainMap;
	}


#endregion

  //Flood-fill (node):
  //1. Set Q to the empty queue or stack.
  //2. Add node to the end of Q.
  //3. While Q is not empty:
  //4.   Set n equal to the first element of Q.
  //5.   Remove first element from Q.
  //6.   If n is Inside:
  //       Set the n
  //       Add the node to the west of n to the end of Q.
  //       Add the node to the east of n to the end of Q.
  //       Add the node to the north of n to the end of Q.
  //       Add the node to the south of n to the end of Q.
  //7. Continue looping until Q is exhausted.
  //8. Return.

  //Returns root node
  public CPFNode GenerateDirectedGraphFromFloodFill(Vector2 startingPoint)
  {
    int NodesChecked = 0;
    rootNode.pos = startingPoint;

    NodeQueueSorter sorter = new NodeQueueSorter(rootNode);

    //1. Set Q to the empty queue or stack.
    //Initialize the queue to be sorted via the absolute distance from the root node and the nodes that way we search in a radial pattern instead of diamond or line
    HashSet<CPFNode> queue = new HashSet<CPFNode>();
    HashSet<Vector2> checkedPos = new HashSet<Vector2>();

    //2. Add node to the end of Q.
    queue.Add(rootNode);
    checkedPos.Add(rootNode.pos);
    //rootNode.state = NodeState.Closed;

    //3. While Q is not empty:
    while(queue.Count != 0)
    {
      //4.   Set n equal to the first element of Q.
      CPFNode currNode = queue.First();
      //5.   Remove first element from Q.
      //Console.WriteLine("Checking Node: " + currNode.pos.x.ToString() + ", " + currNode.pos.y.ToString());
      //Console.WriteLine("Queue Size: " + queue.Count.ToString());
      NodesChecked++;
      queue.Remove(currNode);
      checkedPos.Add(currNode.pos);
      
      //6.   If n is Inside: (aInside = exists within the checked values but we are not adding checked values into the list so skip)
      //       Set the n

      //Make sure we are adding actual terrain not walls
      //       Add the node to the west of n to the end of Q.

      for(int i = -1; i <= 1; ++i)  
      {
        for(int j = -1; j <= 1; ++j)  
        {
          //Ignore center (this node) and corners (only doing + shape)
          if((i == 0 && j == 0) || (i != 0 && j != 0))
            continue;

          if(terrainMap[(int)currNode.pos.x + i, (int)currNode.pos.y + j] == 0)
          {
            Vector2 checkPos = new Vector2(currNode.pos.x + i, currNode.pos.y + j);
            if(!checkedPos.TryGetValue(checkPos, out checkPos))
            {
              //If node doesn't exist then create new node
              CPFNode newNode = new CPFNode();
              newNode.pos = new Vector2(currNode.pos.x + i, currNode.pos.y + j);
              newNode.parent = currNode;
              currNode.children.Add(newNode);
              queue.Add(newNode);
            }
          }
        }
      }
      //Finish up the rest of the values
      //       Add the node to the east of n to the end of Q.
      //       Add the node to the north of n to the end of Q.
      //       Add the node to the south of n to the end of Q.
      //7. Continue looping until Q is exhausted.
      //8. Return.

    }
    
    UpdateDirectedMapVis(rootNode);
    return rootNode;
  }

  public void Clear()
  {
    directedGraphVisMap.Clear();
    roomVisMap.Clear();
  }


  //DFS recursive func to iter over every child and show its parent relationship
  void UpdateDirectedMapVis(CPFNode currNode)
  {
    Vector2 offsetToParent= new Vector2(0,0);
    if(currNode.parent != null)
    {
      //parent - child = dir child->parent
      offsetToParent = currNode.parent.pos - currNode.pos;
    }
    
    directedGraphVisMap.SetCell((int)-currNode.pos.x + width / 2, (int)-currNode.pos.y + height / 2, DebugDirToTile[offsetToParent]);
    foreach (var child in currNode.children)
    {
      UpdateDirectedMapVis(child);
    }
  }
}
