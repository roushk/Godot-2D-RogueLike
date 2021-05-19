using Godot;
using System;
using Godot.Collections;

public class TileSetAnimation : TileMap
{
  [Export]
  public TileSet tileSet;

  [Export]
  public TileSettings[] tileSettings;

  [Export]
  public TileSettings singleTileSetting;


  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    foreach(var tileSetting in tileSettings)
    {
      tileSetting.origionalRegion = tileSet.TileGetRegion(tileSetting.tile);
      //seconds per frame = frames per second / seconds
      tileSetting.secondsPerFrame = tileSetting.animFramerate/60.0f;
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    foreach(var tileSetting in tileSettings)
    {
      tileSetting.currentTimePassed += delta;
      if(tileSetting.currentTimePassed > tileSetting.secondsPerFrame)
      {
        tileSetting.currentFrame = (tileSetting.currentFrame + 1) % tileSetting.maxFrame;
        tileSetting.currentTimePassed = 0;
      }

      Rect2 region = tileSet.TileGetRegion(tileSetting.tile);
      region.Position = new Vector2(tileSetting.origionalRegion.Position.x + tileSetting.origionalRegion.Size.x * tileSetting.currentFrame,  tileSetting.origionalRegion.Position.y);

      tileSet.TileSetRegion(tileSetting.tile, region);
    }
  }
}
