using Godot;
using System;
using Godot.Collections;

public class TileSetAnimation : TileMap
{
  [Export]
  public TileSet tileSet;

  //3, 19, 20, 21
  //Arrays of custom types cannot be edited in the inspector currently so doing the next best thing, arrays of the data

  
  public const int maxNumAnimatedTiles = 10;
  [Export] 
  public int numAnimatedTiles = 1;

  //Starting state of the region
  public Rect2[] origionalRegion = new Rect2[maxNumAnimatedTiles];

  //Specific tile inside of the tileset
  [Export]
  public int[] tiles = new int[maxNumAnimatedTiles];
  
  //Max frames of animation
  [Export]
  public int[] maxFrame = new int[maxNumAnimatedTiles]{4,4,4,4,4,4,4,4,4,4};
  //internal current frame
  public int[] currentFrame = new int[maxNumAnimatedTiles];
  
  //Framerate of the animation
  [Export]
  public int[] animFramerate = new int[maxNumAnimatedTiles]{8,8,8,8,8,8,8,8,8,8};

  //internal calc to get the seconds per frame for the timer
  public float[] secondsPerFrame = new float[maxNumAnimatedTiles];

  //internal counter of time passed per frame
  public float[] currentTimePassed = new float[maxNumAnimatedTiles];


  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    if(tileSet == null)
    {
      tileSet = (this as TileMap).TileSet;
    }
    for (int i = 0; i < numAnimatedTiles; i++)
    {
      origionalRegion[i] = tileSet.TileGetRegion(tiles[i]);
      //seconds per frame = frames per second / seconds
      secondsPerFrame[i] = animFramerate[i]/60.0f;
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    for (int i = 0; i < numAnimatedTiles; i++)
    {
      currentTimePassed[i] += delta;
      if(currentTimePassed[i] > secondsPerFrame[i])
      {
        currentFrame[i] = (currentFrame[i] + 1) % maxFrame[i];
        currentTimePassed[i] = 0;
      }

      Rect2 region = tileSet.TileGetRegion(tiles[i]);
      region.Position = new Vector2(origionalRegion[i].Position.x + origionalRegion[i].Size.x * currentFrame[i],  origionalRegion[i].Position.y);

      tileSet.TileSetRegion(tiles[i], region);
    }
  }
}

