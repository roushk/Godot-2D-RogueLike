using Godot;
using System;

//can be either a unique item or a material
public class InventoryObject : Node2D
{
  [Export]
  public string inventoryObjectName;

  public bool isMaterial = true;
  public Materials.Material material;
  public int numMaterials = 0;

  public BaseBlueprint blueprint;

  float vaccumRadius = 50.0f;
  float minRadius = 10.0f;
  Vector2 velocity;
  AnimatedSprite animatedSprite;
  Area2D area2D;
  CollisionShape2D collisionShape2D;
  CPUParticles2D cpuParticles2D;
  PlayerTopDown player;

  //be alive for 1 second before enabling collision
  float timeToCollide = 0.5f;

  [Export]
  public bool DrawDebugInfo = false;

  public override void _Ready() 
  {
    animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;
    area2D = GetNode("Area2D") as Area2D;
    collisionShape2D = GetNode("Area2D/CollisionShape2D") as CollisionShape2D;
    cpuParticles2D = GetNode("CPUParticles2D") as CPUParticles2D;
    //To get nodes in main scene from instanced scene need to use this instead of root.getnode
    player = GetTree().CurrentScene.FindNode("Player") as PlayerTopDown;
    collisionShape2D.Disabled = true;
    if(player == null)
    {
      throw new Exception ("Error: Player is null in InventoryObject");
    }
  }

  public override void _Draw()
  {
    if(DrawDebugInfo)
    {
      DrawCircle(Vector2.Zero, vaccumRadius, new Color(1,0,0,0.5f));
    }
  }

  public override void _Process(float delta)
  {
    timeToCollide -= delta;
    if(timeToCollide < 0)
    {
      collisionShape2D.Disabled = false;
    }
    if(timeToCollide < 0 && GlobalPosition.DistanceSquaredTo(player.GlobalPosition) < vaccumRadius*vaccumRadius
    && GlobalPosition.DistanceSquaredTo(player.GlobalPosition) > minRadius * minRadius)
    {
      //Have the lerp speed up the closer to the player the objects are
      GlobalPosition = GlobalPosition.LinearInterpolate(player.GlobalPosition, 0.02f);
    }
  }
}
