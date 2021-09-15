using Godot;
using System;
using System.Collections.Generic;
public class Enemy : CombatCharacter
{
  public AStar.AStarPather pather = new AStar.AStarPather();
  public float minMovementDistance = 10.0f;
  AnimatedSprite sprite;
  CollisionPolygon2D attackCollider;
  CollisionShape2D movementCollider;

  public List<Vector2> movementPath = new List<Vector2>();
  
  Vector2 globalMovementPosGoal;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    sprite = GetNode("AnimatedSprite") as AnimatedSprite;
    attackCollider = GetNode("MovementCollider") as CollisionPolygon2D;
    movementCollider = GetNode("AttackSwingCollider") as CollisionShape2D;
    sprite.Playing = true;
    globalMovementPosGoal = GlobalPosition;
    movementSpeed *= 50f;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    if(GlobalPosition != globalMovementPosGoal || movementPath.Count != 0)
    {

      Vector2 deltaMoveToGoal = globalMovementPosGoal - GlobalPosition;

      //If within the minMoveDist than don't move if greater than move
      if(deltaMoveToGoal.Length() > minMovementDistance)
      {
        deltaMoveToGoal = deltaMoveToGoal.Normalized();
        //Move towards the goal position
        //GlobalPosition = GlobalPosition + deltaMoveToGoal * movementSpeed * delta;
        MoveAndSlide(deltaMoveToGoal * movementSpeed * delta, new Vector2(0,-1));
      }
      //If we still have path to go 
      else if (movementPath.Count > 1)
      {
        //Update current movement node
        globalMovementPosGoal = movementPath[0];
        movementPath.Remove(movementPath[0]);
      }
    }
  }
}
