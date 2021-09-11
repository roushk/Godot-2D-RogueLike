using Godot;
using System;

public class PlayerTopDown : KinematicBody2D
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  Vector2 velocity;
  Vector2 movingDirection;
  float horizontalMovementPower = 1200.0f;
  float verticalMovementPower = 1200.0f;
  
  float idleEpsilon = 10;

  bool grounded = true;
  bool attacking = false;

  FacingDir currentFacing = FacingDir.Up;

  public enum FacingDir
  {
		Up,
		Right,
		Down,
		Left,
  }

  float getDegreeFromFacing(FacingDir dir)
  {
		return (int)dir * 90;
  }

  //map of object name to the amount currently owned
  System.Collections.Generic.SortedDictionary<string, int> materialInventory = new System.Collections.Generic.SortedDictionary<string, int>();
  

  //integer of amount of gold the player has
  int gold = 0;

  //TODO create a UniqueItem class that is any unique item such as a crafted sword, armor, ring, necklace, anything of that nature
  //System.Collections.Generic.List<UniqueItem> uniqueItemInventory;


  //todo Doesnt quite work, need better way to detect if above fallable block
  bool OnTile = false;
  // Called when the node enters the scene tree for the first time.

  OreWorldObject currentlyOverlappedOre;
  bool overlappingOre = false;

  public override void _Ready()
  {
		//GetNode("OreWorldObject");
  }

  public void _on_PlayerInteractionArea_area_entered(Area2D body)
  {
		OreWorldObject ore = body.GetParent() as OreWorldObject;
		if(ore != null)
		{
			overlappingOre = true;
			currentlyOverlappedOre = ore;
		}
		
		InventoryObject inv = body.GetParent() as InventoryObject;
		if(inv != null)
		{
			/*

			if inv == material
			add to materials
			destroy world object

			if inv = unique object
			add to unique object list
			destroy world object

			inventoryObject
			*/
			
		}
  }

  public void _on_PlayerInteractionArea_area_exited(Area2D body)
  {

		OreWorldObject ore = body.GetParent() as OreWorldObject;
		if(ore != null)
		{
			overlappingOre = false;
			currentlyOverlappedOre = ore; 
		}
  }

  public override void _Draw()
  {
	  this.DrawLine(Position,Position + velocity,Color.Color8(1,0,0,1));
	  this.DrawLine(Position,Position + new Vector2(0,50),Color.Color8(0,1,0,1));
  }

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
		movingDirection = new Vector2(0,0);
		AnimatedSprite animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;
		AnimationPlayer weaponAnimPlayer = GetNode("WeaponSprite/WeaponAnimPlayer") as AnimationPlayer;
		RayCast2D raycast2D = GetNode("RayCast2D") as RayCast2D;
		Sprite weaponSprite = GetNode("WeaponSprite") as Sprite;

		raycast2D.CastTo = new Vector2(0,50);

		//the raycast only collides with the second layer so only the floors
		OnTile = raycast2D.IsColliding();

		//update player movement
		if(Godot.Input.IsActionPressed("PlayerUp"))
		{
			currentFacing = FacingDir.Up;
			movingDirection.y -= 1;
			//velocity += new Vector2(0,-verticalMovementPower) * delta;
		}
		if(Godot.Input.IsActionPressed("PlayerDown"))
		{
			currentFacing = FacingDir.Down;
			movingDirection.y += 1;
			//velocity += new Vector2(0,verticalMovementPower) * delta;
		}
		if(Godot.Input.IsActionPressed("PlayerRight"))
		{
			//Prioritize left and right attacks more than up and down
			currentFacing = FacingDir.Right;
			movingDirection.x += 1;
			//velocity += new Vector2(horizontalMovementPower,0) * delta;
		}
		if(Godot.Input.IsActionPressed("PlayerLeft"))
		{
			movingDirection.x -= 1;
			currentFacing = FacingDir.Left;
			//velocity += new Vector2(-horizontalMovementPower,0) * delta;
		}

		velocity = movingDirection.Normalized() * 10000.0f * delta;  

		//if velocity x == 0 then dont change
		if(velocity.x > 0)
		{
			animatedSprite.FlipH = false;
		}
		else if(velocity.x < 0)
		{
			animatedSprite.FlipH = true;
		}
				
		//Attacking
		
		if(Godot.Input.IsActionPressed("PlayerAttack"))
		{
			attacking = true;
			animatedSprite.Play("Character Attack");
			if(!weaponAnimPlayer.IsPlaying())
			{
			Vector2 mousePos = GetGlobalMousePosition();
			//AngleToPoint does what we need, literally dont need to do anything else, sweet
			weaponSprite.Rotation = Position.AngleToPoint(mousePos) - Mathf.Pi/2.0f;
			weaponAnimPlayer.Play("BasicWeaponAttackAnim");
			}
			velocity = new Vector2(0,0);
		}
	
		//TODO move this elseware
		if(currentlyOverlappedOre != null && overlappingOre && attacking)
		{
			currentlyOverlappedOre.GetNode<CPUParticles2D>("CPUParticles2D").Emitting = true;
			currentlyOverlappedOre.timeToMine -= delta;

			//if spent enough time mining ore
			if(currentlyOverlappedOre.timeToMine <= 0)
			{
			//Spawn ore item

			//Destroy ore object
			currentlyOverlappedOre.QueueFree();
			currentlyOverlappedOre = null;
			overlappingOre = false;


			}
		}

		if(IsInstanceValid(currentlyOverlappedOre) && !attacking)
		{
			currentlyOverlappedOre.GetNode<CPUParticles2D>("CPUParticles2D").Emitting = false;
		}


		//Bad, need to find a way to detect if animation is completed
		if(attacking && animatedSprite.Frame == 4)
		{
			attacking = false;
		}
		//elocity.y += gravity * delta;
		
		//idle - if grounded and slow
		if(grounded && !attacking)
		{
			if((Mathf.Abs(velocity.x) > idleEpsilon || Mathf.Abs(velocity.y) > idleEpsilon))
			{
			animatedSprite.Play("Character Run");
			}
			else
			{
			animatedSprite.Play("Character Idle");
			}
		}
		
		//Update velocity last
		velocity = MoveAndSlide(velocity, new Vector2(0,-1)) * 0.80f;
  }
}
