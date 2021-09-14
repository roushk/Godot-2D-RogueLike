using Godot;
using System;

public class CombatCharacter : KinematicBody2D
{
  int maxHealth = 100;
  int currentHealth = 100;
  protected float movementSpeed = 100.0f;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    
  }
}
