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
    OpenList,
    Closed
  }
  
  public class CPFNode
  {
    public Vector2 pos = new Vector2();
    public CPFNode parent;
    public List<CPFNode> children = new List<CPFNode>();
    public NodeState state = NodeState.NotChecked;
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
      if(x.pos.DistanceSquaredTo(rootNode.pos).CompareTo(y.pos.DistanceSquaredTo(rootNode.pos)) != 0)
      {
        return x.pos.DistanceSquaredTo(rootNode.pos).CompareTo(y.pos.DistanceSquaredTo(rootNode.pos));
      }
      else if(x.pos.x.CompareTo(y.pos.x) != 0)
      {
        return x.pos.x.CompareTo(y.pos.x);
      }
      else if(x.pos.y.CompareTo(y.pos.y) != 0)
      {
        return x.pos.y.CompareTo(y.pos.y);
      }
      else if(x.GetHashCode().CompareTo(y.GetHashCode()) != 0)
      {
        return x.GetHashCode().CompareTo(y.GetHashCode());
      }
      else
        return 0;
    }
  }

#endregion

#region Variables
  public const int maxColors = 112;

  //0 is floor
  int [,] terrainMap;
  int width;
  int height;

  Godot.TileMap directedGraphVisMap = new TileMap();
  Godot.TileMap roomVisMap = new TileMap();
  Godot.TileMap KMeansVisMap = new TileMap();
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
    //9 is open list
  };

  NodeQueueSorter comparer;
  //SortedSet<CPFNode> queue = new SortedSet<CPFNode>();
  List<CPFNode> queue = new List<CPFNode>();
  HashSet<Vector2> checkedPos;

  //Clusters C_1 -> C_K
  //Dict of point to centroid
  Dictionary<KeyValuePair<int, int>, int> KMeansSets = new Dictionary<KeyValuePair<int, int>, int>();
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

  public void SetKMeansVisMap(ref Dictionary<string,Godot.TileMap> _mapIDVisualization, string map)
  {
    KMeansVisMap = _mapIDVisualization[map];
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


  //## K-Means Clustering 
  //1. Choose the number of clusters(K) and obtain the data points 
  //2. Place the centroids c_1, c_2, ..... c_k randomly 
  //3. Repeat steps 4 and 5 until convergence or until the end of a fixed number of iterations
  //4. for each data point x_i:
  //       - find the nearest centroid(c_1, c_2 .. c_k) 
  //       - assign the point to that cluster 
  //5. for each cluster j = 1..k
  //       - new centroid = mean of all points assigned to that cluster
  //6. End 

  public bool GenerateKMeansFromTerrain(int numClusters, List<KeyValuePair<int, int>> pointCloud, int numIteratons = 100, bool runIteratively = false)
  {
    //## K-Means Clustering 
    //1. Choose the number of clusters(K) and obtain the data points 
    List<Vector2> centroids = new List<Vector2>();
    
    Random random = new Random();
    
    //2. Place the centroids c_1, c_2, ..... c_k randomly 
    //Generate the centroids from the point cloud randomly, can change the random but for now using basic random
    for (int i = 0; i < numClusters; ++i)
    {
      KeyValuePair<int,int> pt = pointCloud[random.Next(0,pointCloud.Count - 1)];
      centroids.Add(new Vector2(pt.Key, pt.Value));
    }

    //3. Repeat steps 4 and 5 until convergence or until the end of a fixed number of iterations

    for (int i = 0; i < numIteratons; ++i)
    {
    //4. for each data point x_i:
      foreach (var point in pointCloud)
      {
        float closestDistSq = float.MaxValue;
        int closestCentroid = 0;
    //       - find the nearest centroid(c_1, c_2 .. c_k) 
        for (int j = 0; j < centroids.Count; j++)
        {
          //Distance formula squared = (x2-x1)^2 + (y2-y1)^2
          float dist = ((centroids[j].x - point.Key) * (centroids[j].x - point.Key)) + ((centroids[j].y - point.Value) * (centroids[j].y - point.Value));
          
          //Get smallest distance
          if(dist < closestDistSq)
          {
            closestCentroid = j;
            closestDistSq = dist;
          }
        }
    //       - assign the point to that cluster 
        KMeansSets[point] = closestCentroid;
      }

    //5. for each cluster j = 1..k

      Vector2[] centroidSums = new Vector2[centroids.Count];
      int[] centroidCount = new int[centroids.Count];
      foreach (var item in KMeansSets)
      {
        centroidSums[item.Value] += new Vector2(item.Key.Key, item.Key.Value);
        centroidCount[item.Value]++;
      }
      for (int j = 0; j < centroids.Count; j++)
      {
          
        centroids[j] = centroidSums[j] / centroidCount[j];
      }
    //       - new centroid = mean of all points assigned to that cluster
    //mean = sum / num
    }
    //6. End 
    //Centroids for Clusters C_1 -> C_K
    CleanKMeansIslands();
    UpdateKMeansVisMap();
    return false;
  }

  //Find islands and set figure out a way to set them to the closest set
  void CleanKMeansIslands()
  {

  }


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

  //Returns is finished
  public bool GenerateDirectedGraphFromFloodFill(out CPFNode outRootNode, Vector2 startingPoint, bool runIteratively = false)
  {
    int NodesChecked = 0;
    outRootNode = rootNode;
    //Considering queue count = 0 means that we are the first iter
    if(!runIteratively || runIteratively && queue.Count == 0)
    {
      rootNode.pos = startingPoint;
      rootNode.state = NodeState.Closed;
      comparer = new NodeQueueSorter(rootNode);
      //queue = new SortedSet<CPFNode>(comparer);
      //queue = new HashSet<CPFNode>();
      queue = new List<CPFNode>();
      checkedPos = new HashSet<Vector2>();
      //1. Set Q to the empty queue or stack.
      //Initialize the queue to be sorted via the absolute distance from the root node and the nodes that way we search in a radial pattern instead of diamond or line
      

      //2. Add node to the end of Q.
      queue.Add(rootNode);
      checkedPos.Add(rootNode.pos);
      //rootNode.state = NodeState.Closed;
    }

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
      //This is only affecting nodes that later have children added to them???
      currNode.state = NodeState.Closed;
      
      //6.   If n is Inside: (aInside = exists within the checked values but we are not adding checked values into the list so skip)
      //       Set the n

      //Make sure we are adding actual terrain not walls
      //       Add the node to the west of n to the end of Q.
      //Add adjacent 
      for(int i = -1; i <= 1; ++i)  
      {
        for(int j = -1; j <= 1; ++j)  
        {
          //Ignore center (this node) and corners (only doing + shape)
          if((i == 0 && j == 0) || (i != 0 && j != 0))
            continue;

          //0 is floor
          if(terrainMap[(int)currNode.pos.x + i, (int)currNode.pos.y + j] == 0)
          {
            Vector2 checkPos = new Vector2(currNode.pos.x + i, currNode.pos.y + j);

            //If node hasn't been checked yet
            if(!checkedPos.TryGetValue(checkPos, out checkPos))
            {
              //If node doesn't exist then create new node
              CPFNode newNode = new CPFNode();
              newNode.pos = new Vector2(currNode.pos.x + i, currNode.pos.y + j);
              //need to show that we have checked this node.... not adding this allows us to create multiple nodes for each tile
              checkedPos.Add(newNode.pos);
              newNode.parent = currNode;
              newNode.state = NodeState.OpenList;
              currNode.children.Add(newNode);
              //Make sure to add to the end
              queue.Add(newNode);
              //queue.Add(newNode);
              directedGraphVisMap.SetCell((int)currNode.pos.x , (int)currNode.pos.y , 9);  //9 is open list
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

      //If we run iteratively than we want to 
      if(runIteratively)
        break;
    }

    UpdateDirectedMapVis(rootNode);
    if(runIteratively && queue.Count != 0)
      return false;
    if(runIteratively && queue.Count == 0)
      return true;
    
    return true;
  }

  //Resets flood fill data structures + root node
  public void ResetFloodFill()
  {
    if(queue != null)
      queue.Clear();
    if(checkedPos != null)
      checkedPos.Clear();
    if(directedGraphVisMap != null)
      directedGraphVisMap.Clear();
    rootNode = new CPFNode();
  }

  public void Clear()
  {
    if(directedGraphVisMap != null)
      directedGraphVisMap.Clear();
    if(roomVisMap != null)
      roomVisMap.Clear();
    if(KMeansVisMap != null)
      KMeansVisMap.Clear();
    KMeansSets.Clear();
  }

#region Debug Visual Funcs
  //DFS recursive func to iter over every child and show its parent relationship
  public void UpdateDirectedMapVis(CPFNode currNode)
  {
    Vector2 offsetToParent= new Vector2(0,0);
    if(currNode.parent != null)
    {
      //parent - child = dir child->parent
      offsetToParent = currNode.parent.pos - currNode.pos;
    }
    
    if(currNode.state == NodeState.OpenList) 
    {
      directedGraphVisMap.SetCell((int)currNode.pos.x , (int)currNode.pos.y , 9);  //9 is open list
    }
    else
      directedGraphVisMap.SetCell((int)currNode.pos.x , (int)currNode.pos.y , DebugDirToTile[offsetToParent]);

    foreach (var child in currNode.children)
    {
      UpdateDirectedMapVis(child);
    }
  }

  public void UpdateKMeansVisMap()
  {
    //KMeansSet is dict of KV pairs to ID
    foreach (var item in KMeansSets)
    {
      KMeansVisMap.SetCell(item.Key.Key , item.Key.Value , (item.Value * 3) % maxColors);
    }
  }
#endregion
}
