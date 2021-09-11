using Godot;
using System;

public class InventoryObject : Node2D
{
  [Export]
  public string inventoryObjectName;

  int vaccumRadius = 500;
  Vector2 velocity;


  public override void _Ready() 
  {
      
  }

  public override void _Process(float delta)
  {

    AnimatedSprite animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;

    RigidBody2D rigidBody2D = GetNode("RigidBody2D") as RigidBody2D;

    CollisionShape2D collisionShape2D = GetNode("RigidBody2D/CollisionShape2D") as CollisionShape2D;

    CPUParticles2D cpuParticles2D = GetNode("CPUParticles2D") as CPUParticles2D;

    var player = GetNode("../Player") as PlayerTopDown;
  
    if(Position.DistanceSquaredTo(player.Position) < vaccumRadius)
    {
      velocity += (player.Position - Position).Normalized() * 1.0f / Position.DistanceSquaredTo(player.Position);
    }

    rigidBody2D.LinearVelocity = velocity;
    

  }

}
