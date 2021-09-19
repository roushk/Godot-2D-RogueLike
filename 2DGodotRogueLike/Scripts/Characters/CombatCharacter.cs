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

  public AnimatedSprite animatedSprite;
  public ShaderMaterial shaderMaterial;

  public CharacterType characterType = CharacterType.NonCombat;

  public float damageInvincibilityTimeLeft = 0.0f;
  public float damageMaxInvincibilityTimeLeft {get; protected set;} = 0.5f;

  public float rollInvincibilityTimeLeft = 0.0f;
  public float rollMaxInvincibilityTimeLeft {get; protected set;} = 0.25f;
  
  //default stun duration on enemies after being damaged
  public float damagedStunDuration {get; protected set;} = 0.5f;
  public float currentStunDuration = 0.0f;

  //default dash cooldown
  public float dashCooldownMax {get; protected set;} = 0.5f;
  public float currentDashCooldown = 0.0f;

  protected float baseKnockBack = 500.0f;
  protected float extraKnockback = 1.0f;

  public bool characterDead {get;protected set;} = false;

  protected Vector2 velocity;

  public void CharacterDeadCallback(int damageTakenThatKilled)
  {
    //TODO play actual death animation here
    this.QueueFree();
  }

  public bool DamageCharacter(int damage, Vector2 knockback)
  { 
    //If character can take damage
    if(Mathf.Max(damageInvincibilityTimeLeft,rollInvincibilityTimeLeft) <= 0)
    {
      //Take damage and reset invincibility timer
      damageInvincibilityTimeLeft = damageMaxInvincibilityTimeLeft;
      currentStunDuration = damagedStunDuration;

      currentHealth -= damage;
      if(currentHealth <= 0)
      {
        currentHealth = 0;
        CharacterDeadCallback(damage);
        characterDead = true;
      }
      velocity += knockback;
    }

    return characterDead;
  }

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
    shaderMaterial = animatedSprite.Material as ShaderMaterial;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
    if(rollInvincibilityTimeLeft > 0)
    {
      rollInvincibilityTimeLeft -= delta;
    }

    if(damageInvincibilityTimeLeft > 0)
    {
      StartFlashing();
      damageInvincibilityTimeLeft -= delta;
    }

    if(damageInvincibilityTimeLeft < 0)
    {
      StopFlashing();
      damageInvincibilityTimeLeft = 0;
    }
  }

  protected void StartFlashing()
  {
    if(shaderMaterial != null)
    {
      shaderMaterial.SetShaderParam("flashing", true);
    }
  }

  protected void StopFlashing()
  {
    if(shaderMaterial != null)
    {
      shaderMaterial.SetShaderParam("flashing", false);
    }
  }
}
