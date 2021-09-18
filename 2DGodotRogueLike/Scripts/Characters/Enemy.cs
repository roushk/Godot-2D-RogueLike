using Godot;
using System;
using System.Collections.Generic;
public class Enemy : CombatCharacter
{
  public AStar.AStarPather pather = new AStar.AStarPather();

  //TODO move out this to a MapManager singleton
  public TestLevelGeneration testLevelGeneration;
  PlayerManager playerManager;


  public float minMovementDistance = 2.0f;
  AnimatedSprite sprite;

  CollisionShape2D movementCollider;
  CollisionShape2D touchCollider;
  CollisionPolygon2D attackCollider;


  //Collides with walls only
  RayCast2D rayCast;

  HealthBar healthBar;

  int touchDamage = 10;
  int punchAttackDamage = 15;

  float aggroRadius = 100.0f;

  //distance at which the slime tries to attack the player
  float attackDistance = 10.0f;

  public List<Vector2> movementPath = new List<Vector2>();
  

  Vector2 globalMovementPosGoal;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    base._Ready();
    characterType = CharacterType.Enemy;
    sprite = GetNode("AnimatedSprite") as AnimatedSprite;
    movementCollider = GetNode("MovementCollider") as CollisionShape2D;
    attackCollider = GetNode("AttackArea/AttackAreaCollider") as CollisionPolygon2D;
    touchCollider = GetNode("TouchArea/TouchAreaCollider") as CollisionShape2D;
    healthBar = GetNode("HealthBar") as HealthBar;

    rayCast = GetNode("RayCast2D") as RayCast2D;

    //Set singletons
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    testLevelGeneration = GetNode<TestLevelGeneration>("/root/TestLevelGenNode");

    sprite.Playing = true;
    globalMovementPosGoal = GlobalPosition;
    movementSpeed *= 20f;
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
  public override void _PhysicsProcess(float delta)
  {
    float distanceToPlayerSquared = float.MaxValue;
    if(playerManager.topDownPlayer != null)
      distanceToPlayerSquared = playerManager.topDownPlayer.GlobalPosition.DistanceSquaredTo(GlobalPosition);

    //If player within aggro radius
    if(distanceToPlayerSquared < aggroRadius * aggroRadius && distanceToPlayerSquared > attackDistance * attackDistance)
    {
      Vector2 directionToPlayer = playerManager.topDownPlayer.GlobalPosition - GlobalPosition;
      //Cast to the direction vector B-A = AB
      rayCast.CastTo = directionToPlayer;
      
      if(rayCast.IsColliding())
      {
        var collidingWith = rayCast.GetCollider();
        PlayerTopDown player = collidingWith as PlayerTopDown;
        TileMap tileMap = collidingWith as TileMap;

        //If player then move towards the player
        if(player != null)
        {
          //Set movement goal
          globalMovementPosGoal = playerManager.topDownPlayer.GlobalPosition;
          //MoveAndSlide(directionToPlayer.Normalized() * movementSpeed * delta, new Vector2(0,-1));
        }
        //If colliding with wall then try to A* to the player
        //Only do it when we have followed the path already
        else if (tileMap != null && movementPath.Count == 0 && GlobalPosition != globalMovementPosGoal)
        {
          pather.InitPather(testLevelGeneration.ForegroundMap.WorldToMap(GlobalPosition), testLevelGeneration.ForegroundMap.WorldToMap(globalMovementPosGoal),
            new AStar.AStarMap(testLevelGeneration.terrainMap, testLevelGeneration.width, testLevelGeneration.height));

          if(pather.GeneratePath() == AStar.PathState.Found)
          {
            List<Vector2> worldPosPath = new List<Vector2>();
            
            //Translate to world position
            foreach (var item in pather.path)
            {
              worldPosPath.Add(testLevelGeneration.ForegroundMap.MapToWorld(item));
            }
            movementPath = worldPosPath;
            globalMovementPosGoal = movementPath[0];
            movementPath.Remove(movementPath[0]);
          }
        }
      }
    }
    
    base._PhysicsProcess(delta);
    
    healthBar.SetHealth(currentHealth);

    //TODO patrol route that goes between multiple points without removing them, maybe pops from and pushes it back if patroling
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
