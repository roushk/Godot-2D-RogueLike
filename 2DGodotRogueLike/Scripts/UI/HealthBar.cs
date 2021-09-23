using Godot;
using System;

public class HealthBar : Control
{
  TextureProgress healthBarUIElement;

  public override void _Ready()
  {
	  healthBarUIElement = GetNode("HealthBarProgress") as TextureProgress;
  }
  
  //Sets the current health of the UI element
  public void SetHealth(float health)
  {
	  healthBarUIElement.Value = health;
  }

  //Set max and current health in the UI element
  public void SetMaxHealth(int maxHealth)
  {
	  healthBarUIElement.Value = maxHealth;
	  healthBarUIElement.MaxValue = maxHealth;
  }
}
