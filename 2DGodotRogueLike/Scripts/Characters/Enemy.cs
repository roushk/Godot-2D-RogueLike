using Godot;
using System;

public class Enemy : CombatCharacter
{
  AnimatedSprite sprite;
  CollisionPolygon2D attackCollider;
  CollisionShape2D movementCollider;


  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    sprite = GetNode("AnimatedSprite") as AnimatedSprite;
    attackCollider = GetNode("MovementCollider") as CollisionPolygon2D;
    movementCollider = GetNode("AttackSwingCollider") as CollisionShape2D;
    sprite.Playing = true;
    
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    
  }
}
