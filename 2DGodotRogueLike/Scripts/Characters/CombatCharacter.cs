using Godot;
using System;

public class CombatCharacter : KinematicBody2D
{
  public int maxHealth = 100;
  public int currentHealth = 100;
  public float movementSpeed = 100.0f;

  public enum CharacterType
  {
    Enemy,
    Player,
    NonCombat
  };

  public CharacterType characterType = CharacterType.NonCombat;

  public float invincibilityTimeLeft = 0.0f;
  public float damageMaxInvincibilityTimeLeft = 0.25f;
  public float rollMaxInvincibilityTimeLeft = 0.5f;

  public bool characterDead {get;protected set;} = false;


  public void CharacterDeadCallback(int damageTakenThatKilled)
  {
    this.QueueFree();
  }

  public bool DamageCharacter(int damage)
  {
    currentHealth -= damage;
    if(currentHealth <= 0)
    {
      currentHealth = 0;
      CharacterDeadCallback(damage);
    }
    
    characterDead = true;
    return characterDead;
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
    if(invincibilityTimeLeft > 0)
      invincibilityTimeLeft -= delta;

      //For some reason its begins set to negative number thats *not* <= 0
    if(invincibilityTimeLeft < 0)
      invincibilityTimeLeft = 0;
  }
}
