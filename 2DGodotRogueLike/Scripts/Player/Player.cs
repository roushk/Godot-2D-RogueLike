using Godot;
using System;

public class Player : KinematicBody2D
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  Vector2 velocity;
  float gravity = 1300f;
  float horizontalMovementPower = 1000.0f;
  float jumpPower = -720.0f;
  
  float idleEpsilon = 10;

  bool grounded = true;


  //todo Doesnt quite work, need better way to detect if above fallable block
  bool OnTile = false;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    
  }


  public override void _Draw()
  {
      this.DrawLine(Position,Position + velocity,Color.Color8(1,0,0,1));
      this.DrawLine(Position,Position + new Vector2(0,50),Color.Color8(0,1,0,1));

  }
//  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {

    AnimatedSprite animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;
    RayCast2D raycast2D = GetNode("RayCast2D") as RayCast2D;

    raycast2D.CastTo = new Vector2(0,50);

    //the raycast only collides with the second layer so only the floors
    grounded = IsOnFloor();
    OnTile = raycast2D.IsColliding();

    //update player movement
    if(Godot.Input.IsActionPressed("PlayerUp") && grounded)
    {
      velocity += new Vector2(0,jumpPower);
    }
    if(Godot.Input.IsActionPressed("PlayerDown"))
    {
      Position = new Vector2(Position.x, Position.y + 1);
    }
    if(Godot.Input.IsActionPressed("PlayerRight"))
    {
      velocity += new Vector2(horizontalMovementPower,0) * delta;
    }
    if(Godot.Input.IsActionPressed("PlayerLeft"))
    {
      velocity += new Vector2(-horizontalMovementPower,0) * delta;
    }

    velocity.y += gravity * delta;

    
    velocity = MoveAndSlide(velocity, new Vector2(0,-1)) * 0.95f;
    if(grounded)
    {
      velocity.x *= 0.90f;
    }


  //if velocity x == 0 then dont change
    if(velocity.x > 0)
    {
      animatedSprite.FlipH = false;
    }
    else if(velocity.x < 0)
    {
      animatedSprite.FlipH = true;
    }
    
   
    //idle - if grounded and slow
    if(grounded)
    {
      if(velocity.x < idleEpsilon && velocity.x > -idleEpsilon)
      {
        animatedSprite.Play("Character Idle");
      }
      else
      {
        animatedSprite.Play("Character Run");
      }
    }
    //if not grounded
    else if(!OnTile)
    {
      if(velocity.y < idleEpsilon && velocity.y > -idleEpsilon)
      {
        animatedSprite.Play("Character Jump");
      }
      else 
      {
        animatedSprite.Play("Character Fall");
      }
    }
  }
 
}
