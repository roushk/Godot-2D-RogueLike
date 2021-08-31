using Godot;
using System;
using System.Collections.Generic;

/*
8 Transformation forms of a single tile, first line is 0 rotated counterclockwise by 90 degrees
the second row is formed by Y axis mirrored images of the top row
 0   1   2   3
XX  X0  0X  XX 
X0  XX  XX  0X

 4   5   6   7
XX  0X  X0  XX 
0X  XX  XX  X0
*/

public enum SymmetryType
{
  No_Sym,                 //No symmetry           
  Centrosym,              //Spin 180 deg sym
  VertAxisSym,            //Vertical axis
  CounterDiagAxisSym,     //y=x Upper right to lower left
  HorzAxisSym,            //Horizontal axis
  MainDiagAxisSym,        //y=-x Upper left to lower right
  HorzAndVertAxisSym,     //Both horizontal and vertical axis symmetry
  DoubleDiagAxisSym,      //y=x, and y=-x axis sym
  All_Sym                 //Symmetrical around all rotations and mirrors
}

public enum SocketType
{
  Road,
  Water,
  Cliff,

}

//Wave Function Collapse Simple Tiled Model Level single tile that contains symmetry and adjacency data for a tile
public class WFC2DTile : Node
{
  public SocketType northSocketType;
  public SocketType southSocketType;
  public SocketType eastSocketType;
  public SocketType westSocketType;
}
