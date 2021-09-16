using Godot;
using System;
using System.Collections.Generic;
public class Enemy : CombatCharacter
{
  public AStar.AStarPather pather = new AStar.AStarPather();
  public float minMovementDistance = 10.0f;
  AnimatedSprite sprite;

  CollisionShape2D movementCollider;
  CollisionShape2D touchCollider;
  CollisionPolygon2D attackCollider;

  HealthBar healthBar;

  int touchDamage = 10;
  int punchAttackDamage = 15;

  public List<Vector2> movementPath = new List<Vector2>();
  
  Vector2 globalMovementPosGoal;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    characterType = CharacterType.Enemy;
    sprite = GetNode("AnimatedSprite") as AnimatedSprite;
    movementCollider = GetNode("MovementCollider") as CollisionShape2D;
    attackCollider = GetNode("AttackArea/AttackAreaCollider") as CollisionPolygon2D;
    touchCollider = GetNode("TouchArea/TouchAreaCollider") as CollisionShape2D;
    healthBar = GetNode("HealthBar") as HealthBar;

    sprite.Playing = true;
    globalMovementPosGoal = GlobalPosition;
    movementSpeed *= 50f;
  }

  //When the touch area overlaps something
  public void _on_TouchArea_body_entered(Node body)
  {
    CombatCharacter character = body as CombatCharacter;
    if(character != null && character.characterType == CharacterType.Player)
    {
      //If character can take damage
      if(character.invincibilityTimeLeft <= 0)
      {
        //Take damage and reset invincibility timer
        character.DamageCharacter(touchDamage);
        character.invincibilityTimeLeft = character.damageMaxInvincibilityTimeLeft;
      }
    }
  }

  //When the punch area overlaps
  public void _on_PunchArea_body_entered(Node body)
  {
    CombatCharacter character = body as CombatCharacter;
    if(character != null && character.characterType == CharacterType.Player)
    {
      //If character can take damage
      if(character.invincibilityTimeLeft <= 0)
      {
        //Take damage and reset invincibility timer
        character.DamageCharacter(punchAttackDamage);
        character.invincibilityTimeLeft = character.damageMaxInvincibilityTimeLeft;
      }
    }
  }
  
  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    
    CombatCharacterProcess(delta);
    
    healthBar.SetHealth(currentHealth);

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
