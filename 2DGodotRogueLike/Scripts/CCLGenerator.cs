using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


//Connected Component Labeling Generator using the Game of Life as the basis for initial level gen
public class CCLGenerator
{
	public enum VisualizeMode
	{
		Root,
		Individual
	}

	const int maxColors = 112;
  int [,] terrainMap;
	int width;
  int height;

  Godot.TileMap mapIDVisualization = new Godot.TileMap(); 

	//This long line is a dictionary from Id's to a hashset (unique list) of integer x,y coords as a keypair
	Dictionary<KeyValuePair<int, int>, int> dictIDToListofCoords = new Dictionary<KeyValuePair<int, int>, int>();

	//dictionary of int ID of set to ID of parent set 
	//key = ID of node, value = ID of parent
	Dictionary<int,int> IDToParent = new Dictionary<int, int>();
	
	//TODO it looks like the example uses a list of pairs and a list of roots
	//a root is an ID that is the value of a pair but if its the key to a pair remove it
	
	const int maxNumGroups = 100000;
	//ID to size for the first iteration
	int [] sizeOfPixelGroup = new int [maxNumGroups];

	public void SetVisualizationMap(ref Godot.TileMap _mapIDVisualization)
	{
		mapIDVisualization = _mapIDVisualization;
	}

	public void UpdateMap(int _width, int _height, ref int [,] _terrainMap)
	{
		width = _width;
		height = _height;
		terrainMap = _terrainMap;
	}

	public void Clear()
	{
		mapIDVisualization.Clear();
		IDToParent.Clear();
		dictIDToListofCoords.Clear();

		for(int i = 0; i < maxNumGroups; i++)
		{
			sizeOfPixelGroup[i] = 0;
		}
	}

	public void DebugPrintIDTree(int treeCount)
	{
		//Debug Print a tree of each current ID
		for(int i = 0; i < treeCount; i++)
		{
			//Print the parent path
			int initialID = i;
			int ID = i;
			int IDout = 0;
			//while we can get parents follow the path
			//Get Root
			string treeRoot = "Following Tree " + ID.ToString();
			while(IDToParent.TryGetValue(ID, out IDout))
			{
				//Console.WriteLine(" ->" + IDout.ToString());
				treeRoot += " -> " + IDout.ToString();
				//follow tree up
				ID = IDout;
			}

			GD.Print(treeRoot);
		}
	}

	public void UpdateIDTreeToRoots(ref int [] sizeOfPixelGroup)
	{

	}
	public void VisualizeIDTree(VisualizeMode mode)
	{
		//put all the pixels into this new map so that we have the links between each pixel and each set still
		//Dictionary<KeyValuePair<int, int>, int> listOfCoords = new Dictionary<KeyValuePair<int, int>, int>(dictIDToListofCoords);

		//for every pixel
		for(int i = 0; i < dictIDToListofCoords.Count; ++i)
		{
			
			var element = dictIDToListofCoords.ElementAt(i);
			var key = element.Key;
			var value = element.Value;

			int initialID = element.Value;
			int ID = element.Value;
			int IDout = 0;

			if(mode == CCLGenerator.VisualizeMode.Root)
			{
				//Get Root
				while(IDToParent.TryGetValue(ID, out IDout))
				{
					//follow tree up
					ID = IDout;
				}
				//move this 1 pixel to its parent size
				//sizeOfPixelGroup[ID]++;
				//sizeOfPixelGroup[initialID]--;

				//Update the pair to ID 
				//listOfCoords[key] = ID;
			}

			//here when this set has no parent
			mapIDVisualization.SetCell(-key.Key + width / 2, -key.Value + width / 2, ID % maxColors);
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
				
				bool leftPixelExists = dictIDToListofCoords.TryGetValue(new KeyValuePair<int, int>(x - 1, y), out leftPixelID);
				bool abovePixelExists = dictIDToListofCoords.TryGetValue(new KeyValuePair<int, int>(x, y - 1), out abovePixelID);
				
				int leftPixelCurrentParentID = 0;
				bool leftHasParent = IDToParent.TryGetValue(leftPixelID, out leftPixelCurrentParentID);
				
				int abovePixelCurrentParentID = 0;
				bool aboveHasParent = IDToParent.TryGetValue(abovePixelID, out abovePixelCurrentParentID);

				if(leftPixelExists && abovePixelExists)
				{
					//Parent is always smaller than child
					if(leftPixelID < abovePixelID)
					{
						//it doesnt have the parent or it has a parent and the left pixel id is less than the above pixel id
						if(!aboveHasParent || (aboveHasParent && leftPixelID < abovePixelID))
						{
							//set the id to the left pixel
							IDToParent[abovePixelID] = leftPixelID;
						}
					}
					else if(leftPixelID > abovePixelID)
					{
						if(!leftHasParent || (leftHasParent && abovePixelID < leftPixelID))
						{
							IDToParent[leftPixelID] = abovePixelID;
						}
					}
					//else dont set same ID to child of itself

					//leftPixelID size > abovePixelID size
					if(sizeOfPixelGroup[leftPixelID] > sizeOfPixelGroup[abovePixelID])
					{
						//Add to the larger set
						sizeOfPixelGroup[leftPixelID]++;
						dictIDToListofCoords[new KeyValuePair<int, int>(x,y)] = leftPixelID;
					}
					else //leftPixelID size <= abovePixelID size
					{
						//Add to either set
						sizeOfPixelGroup[abovePixelID]++;
						dictIDToListofCoords[new KeyValuePair<int, int>(x,y)] = abovePixelID;
					}
				}
				//now we decide to add to leftPixelID first if possible
				else if(leftPixelExists && !abovePixelExists) //redundant !abovePixelExists for clarity
				{
					sizeOfPixelGroup[leftPixelID]++;
					//if left key than set to left key
					dictIDToListofCoords[new KeyValuePair<int, int>(x,y)] = leftPixelID;
				}
				//If cannot get left key than check up key
				else if(abovePixelExists && !leftPixelExists)  //redundant !leftPixelExists for clarity
				{
					sizeOfPixelGroup[abovePixelID]++;
					//if up key than set to up key
					dictIDToListofCoords[new KeyValuePair<int, int>(x,y)] = abovePixelID;
				}
				//if cannot get left OR up key than create new key
				else
				{
					dictIDToListofCoords[new KeyValuePair<int, int>(x,y)] = nextNewPixelID++;
					sizeOfPixelGroup[nextNewPixelID] = 1;
				}
			}
		}
		
		Console.WriteLine("Max ID = " +  nextNewPixelID.ToString());

		DebugPrintIDTree(nextNewPixelID);

		VisualizeIDTree(VisualizeMode.Root);
  }

}
