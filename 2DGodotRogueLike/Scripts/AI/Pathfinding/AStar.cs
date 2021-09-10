using Godot;
using System;
using System.Collections.Generic;


//General AStar stuff
public class AStar
{
  public enum NodeState
  {
    InvalidState,
    ClosedList,
    OpenList,
    WallNode,
    NotChecked
  }

  //A single node in the AStar Map
  public class AStarNode
  {
    public Vector2 pos = new Vector2(0,0);
    public AStarNode parent = null;
    public NodeState state = NodeState.NotChecked;

    public void UpdateHeuristic(Vector2 goal)
    {
      Vector2 diff = new Vector2(Mathf.Abs(goal.x - pos.x), Mathf.Abs(goal.y - pos.y));

      //1.41421356237 is sqrt of 2
      heuristic = Mathf.Min(diff.x, diff.y) * 1.41421356237f + Mathf.Max(diff.x, diff.y) - Math.Min(diff.x, diff.y);
    }

    //Doing all nodes weighted the same, will potentially have a dict of nodes to their weights if later add different weight nodes
    public static float weight = 1;
    public float givenCost = 0;
    float heuristic = 0;
  }


  public class AStarNodeComparer : Comparer<AStarNode>
  {
    Vector2 endPos;
    //Sort so the last node is the lowest heuristic 
    public override int Compare(AStarNode x, AStarNode y)
    {
      //Comparator defines the values based on heuristic then x/y
      return 0;
    }
    public AStarNodeComparer(Vector2 _endPos)
    {
      endPos = _endPos;
    }
  }

  //Contains data for AStar map
  public class AStarMap
  {
    public int [,] terrainMap;
    public int width;
    public int height;
    AStarNode [,] nodeMap;

    //Sets internal map and populates the AStar node map
    public void SetTerrainMap(int [,] newTerrainMap, int _width, int _height)
    {
      terrainMap = newTerrainMap;
      width = _width;
      height = _height;
      nodeMap = new AStarNode[width, height];

      for (int i = 0; i < width; i++)
      {
        for (int j = 0; j < height; j++)
        {
          //Initialize nodes 
          AStarNode newNode = new AStarNode();
          newNode.pos = new Vector2(i,j);
          if(terrainMap[width,height] == 1)
          {
            newNode.state = NodeState.WallNode;
          }
          nodeMap[i,j] = newNode;
        }
      }
    }

    public AStarNode GetNodeAt(Vector2 pos)
    {
      return nodeMap[(int)pos.x, (int)pos.y];
    }
  }
  
  //Paths through an AStarMap
  public class AStarPather
  {
    AStarMap map;
    SortedSet<AStarNode> openList;
    
    //StartPos, EndPos, and path are relative to the terrain map
    List<Vector2> GeneratePath(Vector2 startPos, Vector2 goalPos, AStarMap _map)
    {
      bool pathFound = false;
      map = _map;
      List<Vector2> path = new List<Vector2>();
      AStarNodeComparer comparer = new AStarNodeComparer(goalPos);
      //Simple path
      if(startPos == goalPos)
      {
        path.Add(goalPos);
        return path;
      }

      openList = new SortedSet<AStarNode>(comparer);
      
      //Start with first node and update its state
      AStarNode startingNode = map.GetNodeAt(startPos);
      startingNode.state = NodeState.OpenList;
      startingNode.UpdateHeuristic(goalPos);
      openList.Add(startingNode);

      while(openList.Count > 0 && pathFound == false)
      {
        //Comparator defines the values based on heuristic then x/y
        AStarNode currentNode = openList.Min;
        currentNode.state = NodeState.ClosedList;

        if(currentNode.pos == goalPos)
        {
          for (int i = -1; i <= 1; i++)
          {
            for (int j = -1; j <= 1; j++)
            {
              //Ignore center (this node) and corners (only doing + shape)
              if((i == 0 && j == 0) || (i != 0 && j != 0))
                continue;
              AStarNode iterNode = map.GetNodeAt(new Vector2(currentNode.pos.x + i,currentNode.pos.y + j));
              if(iterNode.state == NodeState.WallNode)
                continue;

              //if not checked then update the heuristic
              if(iterNode.state == NodeState.NotChecked)
              {
                iterNode.UpdateHeuristic(goalPos);
              }

              if(iterNode.state != NodeState.ClosedList)
              {
                //TODO decide which is cheaper, new potential parent and given or current one
                //iterNode.parent = currentNode;
                //iterNode.givenCost = currentNode.givenCost + 1; //Not doing diagonal so no need to worry about diag cost, which is sqrt2
                if(iterNode.state != NodeState.OpenList)
                {
                  iterNode.state = NodeState.OpenList;
                  openList.Add(iterNode);
                }
                else
                {
                  //Decide between cheaper parents, this new one or current
                }
              }
            }
          }
        }
      }
      
    }
  }


}
