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

  public enum NodeState
  {
    NotChecked,
    Closed
  }
  
  public class Node
  {
    public Vector2 pos = new Vector2();
    public Node parent = new Node();
    public List<Node> children = new List<Node>();
    public NodeState state = NodeState.NotChecked;
  };

  public class NodeQueueSorter : Comparer<Node>
  {
    Node rootNode;
    public NodeQueueSorter(Node _rootNode)
    {
      rootNode = _rootNode;
    }
    public override int Compare(Node x, Node y)
    {
      return x.pos.DistanceSquaredTo(rootNode.pos).CompareTo(y.pos.DistanceSquaredTo(rootNode.pos));
    }
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

  //Returns root node
  public Node GenerateDirectedGraphFromFloodFill(int[,] terrainMap, Vector2 startingPoint)
  {
    Node rootNode = new Node();
    rootNode.pos = startingPoint;

    NodeQueueSorter sorter = new NodeQueueSorter(rootNode);

    //1. Set Q to the empty queue or stack.
    //Initialize the queue to be sorted via the absolute distance from the root node and the nodes that way we search in a radial pattern instead of diamond or line
    SortedSet<Node> queue = new SortedSet<Node>(sorter);

    //2. Add node to the end of Q.
    queue.Add(rootNode);

    //3. While Q is not empty:
    while(queue.Count != 0)
    {
      //4.   Set n equal to the first element of Q.
      Node currNode = queue.First();
      //5.   Remove first element from Q.
      queue.Remove(currNode);
      //6.   If n is Inside: (Inside = exists within the checked values but we are not adding checked values into the list so skip)
      //       Set the n

      //Make sure we are adding actual terrain not walls
      //       Add the node to the west of n to the end of Q.
      if(terrainMap[(int)currNode.pos.x, (int)currNode.pos.y + 1] == 0)
      {
        Node foundNode;
        if(queue.TryGetValue(currNode, out foundNode))
        {
          if(foundNode.state == NodeState.NotChecked)
          {
            queue.Add(foundNode);
          }
        }
        else
        {
          //If node doesn't exist then create new node
          foundNode = new Node();
          queue.Add(foundNode);

        }
      }
      //Finish up the rest of the values
      //       Add the node to the east of n to the end of Q.
      //       Add the node to the north of n to the end of Q.
      //       Add the node to the south of n to the end of Q.
      //7. Continue looping until Q is exhausted.
      //8. Return.

    }


    return rootNode;
  }
}
