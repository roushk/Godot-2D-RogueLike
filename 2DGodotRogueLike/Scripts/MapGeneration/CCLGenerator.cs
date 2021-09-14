using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


//Connected Component Labeling Generator using the Game of Life as the basis for initial level gen
public class CCLGenerator
{
  public enum VisualizeMode
  {
    Root,
    Individual
  }

  public const int maxColors = 47;
  int [,] terrainMap;
  int width;
  int height;

  Godot.TileMap mapIDVisualization = new TileMap();

  //This long line is a dictionary from Id's to a hashset (unique list) of integer x,y coords as a keypair
  Dictionary<KeyValuePair<int, int>, int> dictIDToListOfCoords = new Dictionary<KeyValuePair<int, int>, int>();

  //This dictionary is a list of the final connected sets (caves) ID's to a list of x/y values, basically an invert of the dictIDToListofCoords
  Dictionary<int, List<KeyValuePair<int, int>>> connectedSets = new Dictionary<int, List<KeyValuePair<int, int>>>();

  //dictionary of int ID of set to ID of parent set 
  //Needs to support N to N values guaranteed unique so using a HastSet of Pairs
  //key = ID of node, value = ID of parent
  HashSet<KeyValuePair<int,int>> IDToParent = new HashSet<KeyValuePair<int,int>>();
  
  //TODO it looks like the example uses a list of pairs and a list of roots
  //a root is an ID that is the value of a pair but if its the key to a pair remove it
  
  const int maxNumGroups = 100000;
  //ID to size for the first iteration
  int [] sizeOfPixelGroup = new int [maxNumGroups];

  public void SetVisualizationMap(ref Dictionary<string,Godot.TileMap> _mapIDVisualization, string map)
  {
    mapIDVisualization = _mapIDVisualization[map];
  }

  public void UpdateInternalMap(int _width, int _height, ref int [,] _terrainMap)
  {
    width = _width;
    height = _height;
    terrainMap = _terrainMap;
  }

  public void Clear()
  {
    mapIDVisualization.Clear();
    IDToParent.Clear();
    dictIDToListOfCoords.Clear();

    for(int i = 0; i < maxNumGroups; i++)
    {
      sizeOfPixelGroup[i] = 0;
    }
  }

  //Becomes significantly less useful with hashset of pairs
  public void DebugPrintIDTree(int treeCount) 
  {
    //Debug Print a tree of each current ID
    for(int i = 0; i < treeCount; i++)
    {
      //Print the parent path
      int initialID = i;
      int ID = i;
      int IDParent = 0;
      //while we can get parents follow the path
      //Get Root
      string treeRoot = "Following Tree " + ID.ToString();
      foreach (var IDPair in IDToParent)
      {
        if(IDPair.Key == ID)
        {
          IDParent = IDPair.Value;
          //Console.WriteLine(" ->" + IDout.ToString());
          treeRoot += " -> " + IDParent.ToString();
          //follow tree up
          ID = IDParent;
        }
      }
      GD.Print(treeRoot);
    }
  }

  public void VisualizeIDTree(VisualizeMode mode)
  {
    //put all the pixels into this new map so that we have the links between each pixel and each set still
    //Dictionary<KeyValuePair<int, int>, int> listOfCoords = new Dictionary<KeyValuePair<int, int>, int>(dictIDToListofCoords);

    //for every pixel
    for(int i = 0; i < dictIDToListOfCoords.Count; ++i)
    {
      
      var element = dictIDToListOfCoords.ElementAt(i);
      var key = element.Key;
      var value = element.Value;

      int initialID = element.Value;
      int ID = element.Value;
      int IDout = 0;

      if(mode == CCLGenerator.VisualizeMode.Root)
      {
        //Get Root

        foreach (var IDPair in IDToParent)
        {
          if(IDPair.Key == ID)
          {

            ID = IDout;
          }
        }
        //move this 1 pixel to its parent size
        //sizeOfPixelGroup[ID]++;
        //sizeOfPixelGroup[initialID]--;

        //Update the pair to ID 
        //listOfCoords[key] = ID;
      }

      //here when this set has no parent
      mapIDVisualization.SetCell(key.Key , key.Value , ID % maxColors);
    }

    //update old dict with new one
    //dictIDToListofCoords = listOfCoords;
  }

  //runs Run the actual CCL alg
  public void CCLAlgorithm()
  {
    //Going from top left to bottom right
    //from an arbitary pixel p
    //get the pixel to the left and to the top of p
    // ? X ? 
    // X p ?
    // ? ? ?
    //loop over terrain map

    //********************************************************************//
    //Connected Component Labeling
    //First Pass
    //starting ID is 1 so any with id zero have no parent
    int nextNewPixelID = 1;

    for(int y = 0; y < height; ++y)
    {
      for(int x = 0; x < width; ++x)
      {  
        //ignore walls
        if(terrainMap[x,y] != 0)
          continue;


        //We want to check if there is a pixel to the left and above the current pixel that we are checking  
        int leftPixelID = 0;
        int abovePixelID = 0;
        
        //get the pixel to the left and to the top of p
        // ? X ? 
        // X p ?
        // ? ? ?
        
        bool leftPixelExists = dictIDToListOfCoords.TryGetValue(new KeyValuePair<int, int>(x - 1, y), out leftPixelID);
        bool abovePixelExists = dictIDToListOfCoords.TryGetValue(new KeyValuePair<int, int>(x, y - 1), out abovePixelID);

        if(leftPixelExists && abovePixelExists)
        {
          //Parent is always smaller than child
          if(leftPixelID < abovePixelID)
          {
            IDToParent.Add( new KeyValuePair<int,int>(abovePixelID, leftPixelID));
          }
          else if(leftPixelID > abovePixelID)
          {
            IDToParent.Add( new KeyValuePair<int,int>(leftPixelID, abovePixelID));
          }
          //else dont set same ID to child of itself

          //leftPixelID size > abovePixelID size
          if(sizeOfPixelGroup[leftPixelID] > sizeOfPixelGroup[abovePixelID])
          {
            //Add to the larger set
            sizeOfPixelGroup[leftPixelID]++;
            dictIDToListOfCoords[new KeyValuePair<int, int>(x,y)] = leftPixelID;
          }
          else //leftPixelID size <= abovePixelID size
          {
            //Add to either set
            sizeOfPixelGroup[abovePixelID]++;
            dictIDToListOfCoords[new KeyValuePair<int, int>(x,y)] = abovePixelID;
          }
        }
        //now we decide to add to leftPixelID first if possible
        else if(leftPixelExists && !abovePixelExists) //redundant !abovePixelExists for clarity
        {
          sizeOfPixelGroup[leftPixelID]++;
          //if left key than set to left key
          dictIDToListOfCoords[new KeyValuePair<int, int>(x,y)] = leftPixelID;
        }
        //If cannot get left key than check up key
        else if(abovePixelExists && !leftPixelExists)  //redundant !leftPixelExists for clarity
        {
          sizeOfPixelGroup[abovePixelID]++;
          //if up key than set to up key
          dictIDToListOfCoords[new KeyValuePair<int, int>(x,y)] = abovePixelID;
        }
        //if cannot get left OR up key than create new key
        else
        {
          dictIDToListOfCoords[new KeyValuePair<int, int>(x,y)] = nextNewPixelID++;
          sizeOfPixelGroup[nextNewPixelID] = 1;
        }
      }
    }
    
    //This is the slow part
    //The goal is to collapse all the ID's and parents  and sometimes some fall through the cracks
    while(IDToParent.Count > 0)
    {

      //Need to iterate through every ID and set the ID to the smallest parent value
      //And update the map to set the value to the smallest parent value
      for(int i = 0; i < nextNewPixelID && IDToParent.Count > 0; i++)
      {
        //Set the smallest parent to the current ID value so we never set 1 to something higher than 1
        int smallestParent = i;
        //Get a list where the Key/Value pair is CurrentID/Parent
        foreach(KeyValuePair<int,int> currentIDPair in IDToParent.Where((KeyValuePair<int,int> IDPair) => IDPair.Key == i || IDPair.Value == i))
        {
          if(currentIDPair.Value < smallestParent)
          {
            smallestParent = currentIDPair.Value;
          }
          if(currentIDPair.Key < smallestParent)
          {
            smallestParent = currentIDPair.Key;
          }
        }

        //Set the value of each pair to the smallest
        if(i != smallestParent)
        {
          SetIDToNewID(i, smallestParent);
          //Console.WriteLine("Changes ID " + i .ToString() + " To " + smallestParent.ToString());
        }
      }
    }

    //Console.WriteLine("Max ID = " +  nextNewPixelID.ToString());

    //DebugPrintIDTree(nextNewPixelID);

    VisualizeIDTree(VisualizeMode.Root);
    GenerateConnectedSetLists(nextNewPixelID);
  }

  //Take the dictIDToListofCoords and populate the connectedSets with the series of connected sets and their values
  void GenerateConnectedSetLists(int maxSets)
  {
    //first clear
    connectedSets.Clear();

    foreach (var item in dictIDToListOfCoords)
    {
      //If the ID's set is null create it
      if(!connectedSets.ContainsKey(item.Value))
      {
        connectedSets[item.Value] = new List<KeyValuePair<int, int>>();
      }
      //Add the coordinates to the set at the ID key
      connectedSets[item.Value].Add(item.Key);
    }
  }

  //Returns a copy of the largest set
  public List<KeyValuePair<int, int>> GetLargestSet()
  {
    int largestSetSize = 0;
    int largestSet = 0;

    foreach (var item in connectedSets)
    {
      if(item.Value.Count > largestSetSize)
      {
        largestSet = item.Key;
        largestSetSize = item.Value.Count;
      }
    }
    //https://stackoverflow.com/questions/14007405/how-create-a-new-deep-copy-clone-of-a-listt
    //Returns a new list
    return connectedSets[largestSet].ConvertAll(kvPair => new KeyValuePair<int, int>(kvPair.Key, kvPair.Value));
  }

  //Sets all pixel's IDs from the current ID to the new ID
  void SetIDToNewID(int oldID, int newID)
  {
    //Temp var to not change the collection as we iterate
    List<KeyValuePair<KeyValuePair<int, int>, int>> newValuesToInsert = new List<KeyValuePair<KeyValuePair<int, int>, int>>();

    //Get list of new values
    foreach (var item in dictIDToListOfCoords)
    {
      //If the ID of the coordinates is the current ID then reinsert that pair with the new ID
      if(item.Value == oldID)
        newValuesToInsert.Add(new KeyValuePair<KeyValuePair<int, int>, int>(item.Key, newID));
    }
    
    List<KeyValuePair<int, int>> toDelete = new List<KeyValuePair<int, int>>();
    List<KeyValuePair<int, int>> toAdd = new List<KeyValuePair<int, int>>();
    
    //Create a list of items to remove and items to add to the hash set because no changing while iterating
    //Probably a better way to do this
    foreach (var item in IDToParent)
    {
      //Check Keys
      if(item.Key == oldID)
      {
        toDelete.Add(item);
        //We want to converge at a min set so do not add [1,1] because we know that [1,1] inheriently
        if(item.Value != newID)
          toAdd.Add(new KeyValuePair<int, int>(newID, item.Value));
      }
      //Check Values
      if(item.Value == oldID)
      {
        toDelete.Add(item);
        //We want to converge at a min set so do not add [1,1] because we know that [1,1] inheriently
        if(item.Key != newID)
          toAdd.Add(new KeyValuePair<int, int>(item.Key, newID));
      }
    }

    //Update values
    foreach (var item in toDelete)
    {
      IDToParent.Remove(item);
    }

    foreach (var item in toAdd)
    {
      IDToParent.Add(item);
    }

    //Update Dictionary
    foreach (var item in newValuesToInsert)
    {
      dictIDToListOfCoords[item.Key] = item.Value;
    }
  }
}
