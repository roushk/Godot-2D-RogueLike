using Godot;

public class TileSettings : Node
{
  //Starting state of the region
  public Rect2 origionalRegion;

  //Specific tile inside of the tileset
  [Export]
  public int tile = 0;
  
  //Max frames of animation
  [Export]
  public int maxFrame = 2;
  //internal current frame
  public int currentFrame = 0;
  
  //Framerate of the animation
  [Export]
  public int animFramerate = 8;

  //internal calc to get the seconds per frame for the timer
  public float secondsPerFrame;

  //internal counter of time passed per frame
  public float currentTimePassed;
}
