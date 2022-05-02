using Godot;
using System;
using System.Collections.Generic;

//can be either a unique item or a material
public class InventoryPickupWorldObject : Spatial
{
  [Export]
  public string inventoryObjectName;

  [Export]
  public bool isMaterial = true;
  [Export]
  public Materials.Material material;
  [Export]
  public int numMaterials = 0;

  public Parts.ConstructedWeapon weapon;

  float vaccumRadius = 50.0f;
  float minRadius = 10.0f;
  Vector2 velocity;
  public AnimatedSprite animatedSprite;
  Area2D area2D;
  CollisionShape2D collisionShape2D;
  CPUParticles2D cpuParticles2D;
  PlayerManager playerManager;

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
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    collisionShape2D.Disabled = true;
    animatedSprite.Frame = (int)material;
  }

/*
  public override void _Draw()
  {
    if(DrawDebugInfo)
    {
      DrawCircle(Vector2.Zero, vaccumRadius, new Color(1,0,0,0.5f));
    }
  }
*/

  public override void _Process(float delta)
  {
    timeToCollide -= delta;
    if(timeToCollide < 0)
    {
      collisionShape2D.Disabled = false;
    }
    if(timeToCollide < 0 && Translation.DistanceSquaredTo(playerManager.topDownPlayer.Translation) < vaccumRadius*vaccumRadius
    && Translation.DistanceSquaredTo(playerManager.topDownPlayer.Translation) > minRadius * minRadius)
    {
      //Have the lerp speed up the closer to the player the objects are
      Translation = Translation.LinearInterpolate(playerManager.topDownPlayer.Translation, 0.02f);
    }
  }
}
