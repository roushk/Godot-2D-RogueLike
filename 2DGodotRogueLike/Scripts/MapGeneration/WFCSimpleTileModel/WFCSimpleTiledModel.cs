using Godot;
using System;


/*
Adjacencey list (self adj needs to be listed)
River/River
Grass/Grass
Road/Road
Tree/Tree
Ocean/Ocean
Cliff/Cliff

River/Grass
Grass/Road
Grass/Tree
Tree/Road
Ocean/Cliff

2nd step - overlapping tile connectors (waterfalls, bridges, etc)
Ocean/Waterfall
Waterfall/Cliff

*/


/*
https://robertheaton.com/2018/12/17/wavefunction-collapse-algorithm/
https://github.com/mxgmn/WaveFunctionCollapse#notable-ports-forks-and-spinoffs

Pseudocode for Simple Tiled Model
Each tile is a square constrained by its 4 immediate neighbors defined as sockets 
Simple Tiled Model accounts for symmetry and rotation 
- We are using non rotating tiles so this is moot

We want to allow constraints

1. We need a tile dictionary of each tile and a list of its sockets (up, down, left, right) as an xml doc or something
2. Using that tile dictionary we observe WFC on the map

Simplify the tiles to be single type such as grass can be adj to water + cliff 


In general, a square with high entropy is one with lots of possible tiles remaining in its wavefunction
Every tile starts in a superposition of existing as every single tile
When a tile is collapsed then it updates its neighbors which update its neighbots on potential tiles, if the tiles are 

# Sums are over the weights of each remaining
# allowed tile type for the square whose
# entropy we are calculating.
shannon_entropy_for_square =
  log(sum(weight)) -
  (sum(weight * log(weight)) / sum(weight))

Sparse tree of decisions 

based on the Original Simple Tiled Model Thesis

p47 Find an initial solution and then modify the solution in small blocks
Alg 3.4
It is not
necessary to compute CMt across the entire modelm only the specific modifying block + immediate neighbors, so the array C can be relatively small (Mx + 2)^2
I.E.
Split up scene into smaller subsections because P != NP
? = unobserved
X = observed
Y = neighbor
Z = obs last iter
????????????????????????????    XXXXXY??????????????????????    ZZZZXXXXXY??????????????????
????????????????????????????    XXXXXY??????????????????????    ZZZZXXXXXY??????????????????
???????????????????????????? -> XXXXXY?????????????????????? -> ZZZZXXXXXY?????????????????? ->
????????????????????????????    YYYYY???????????????????????    ZZZZYYYYY???????????????????
????????????????????????????    ????????????????????????????    ????????????????????????????

Compute smaller squares Ri for the entire scene C

while there are states left to observe
  Observation:
    Find a wave element with min non zero entropy
      if no elements with nonzero (either zero or undef go to End:)
  End:
    If the elements are in a completely observed state (all the coefficients except one being zero)
      Return the output
    If the elements are in a contradictory state (all the coefficients being zero)
      finish and return nothing (undo/choose different element from tree)
*/
//Wave Function Collapse Simple Tiled Model Level Generator
public class WFCSimpleTiledModel
{

  int [,] terrainMap;
  int width;
  int height;
  
  //Constraints
  
  public void UpdateInternalMap(int _width, int _height, ref int [,] _terrainMap)
  {
    width = _width;
    height = _height;
    terrainMap = _terrainMap;
  }
  
  public void Iterate()
  {
    
  }

  public void RunIterations(int numIterations)
  {
    //for num iterations
      //iterate
  }

  public void GenerateCompleteMap()
  {
    //while not complete
      //iterate
  }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
