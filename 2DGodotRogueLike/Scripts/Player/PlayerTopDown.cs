using Godot;
using System;

public class PlayerTopDown : CombatCharacter
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  Vector2 velocity;
  Vector2 movingDirection = Vector2.Zero;
	

	//Depricated
  float horizontalMovementPower = 1200.0f;
  float verticalMovementPower = 1200.0f;


	//Scaled multiplier for movespeed to make base 100 a decent speed
	const float movespeedScalar = 50.0f;
  
  float idleEpsilon = 10;

  bool grounded = true;

	[Export]
  bool attacking = false;
	
	//Bool to stop player movement if holding down/spamming the attack button as the anim player is updated after the player so the player can move slightly between each and don't want that
	bool attackingThisFrame = false;
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

	Inventory inventory = new Inventory();

  //todo Doesnt quite work, need better way to detect if above fallable block
  bool OnTile = false;
  // Called when the node enters the scene tree for the first time.

  OreWorldObject currentlyOverlappedOre;
  bool overlappingOre = false;
	Area2D playerArea;
	AnimatedSprite animatedSprite;
	AnimationPlayer weaponAnimPlayer;
	RayCast2D raycast2D;
	Sprite weaponSprite;


	//TODO change to crafted/selected weapon
	public Parts.PartStats weapon = new Parts.PartStats();

  public override void _Ready()
  {
		characterType = CharacterType.Player;
		//GetNode("OreWorldObject");
		playerArea = GetNode<Area2D>("PlayerInteractionArea");
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		weaponAnimPlayer = GetNode<AnimationPlayer>("WeaponSprite/WeaponAnimPlayer");
		raycast2D = GetNode<RayCast2D>("RayCast2D");
		weaponSprite = GetNode<Sprite>("WeaponSprite");

		//Reset attacking sprite
		weaponAnimPlayer.Stop(true);
		attacking = false;

		weapon.baseSlashDamage = 10;
		weapon.baseStabDamage = 10;

	  GetNode<PlayerManager>("/root/PlayerManagerSingletonNode").topDownPlayer = this;

  }


	void CollidingWithInvObject(InventoryObject inv)
	{
		Console.WriteLine("Overlapping Inventory Object" + inv.ToString());
		//Add to relevent material
		if(inv.isMaterial)
		{
			int currVal = 0;
			if(inventory.stackableItems.TryGetValue(inv.material, out currVal))
				inventory.stackableItems[inv.material] = currVal + inv.numMaterials;
		}
		else
		{
			inventory.uniqueItems.Add(new Tuple<string,	BaseBlueprint>(inv.inventoryObjectName,inv.blueprint));
		}

		//TODO setup callback if needed
		//inv.PickedUpCallback();
		//delete the object after adding it
		inv.QueueFree();
	}

	public void _on_PlayerInteractionArea_body_entered(Node body)
  {
		InventoryObject inv = body.GetParent() as InventoryObject;
		if(inv != null)
		{
			CollidingWithInvObject(inv);
		}
	}

	//When the punch area overlaps
  public void _on_SlashArea_body_entered(Node body)
  {
    CombatCharacter character = body as CombatCharacter;
    if(character != null && character.characterType == CharacterType.Enemy)
    {
      //If character can take damage
      if(character.invincibilityTimeLeft <= 0)
      {
        //Take damage and reset invincibility timer
				character.DamageCharacter(weapon.baseSlashDamage);
        character.invincibilityTimeLeft = character.damageMaxInvincibilityTimeLeft;
      }
    }
  }

	//When the punch area overlaps
  public void _on_StabArea_body_entered(Node body)
  {
    CombatCharacter character = body as CombatCharacter;
    if(character != null && character.characterType == CharacterType.Enemy)
    {
      //If character can take damage
      if(character.invincibilityTimeLeft <= 0)
      {
        //Take damage and reset invincibility timer
				character.DamageCharacter(weapon.baseStabDamage);
        character.invincibilityTimeLeft = character.damageMaxInvincibilityTimeLeft;
      }
    }
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
			CollidingWithInvObject(inv);
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
		//For some reason Godot's on entered func just straight up doesnt work if the bodies are moving so 
		//we make them stop moving in their script and run this to verify we got everything and it works most of the time
		//var bodies = playerArea.GetOverlappingBodies();
		//foreach (PhysicsBody2D item in bodies)
		//{
		//	if(item != null)
		//	{
		//		InventoryObject inv = item.GetParent() as InventoryObject;
		//		if(inv != null)
		//		{
		//			CollidingWithInvObject(inv);
		//		}
		//	}
		//}
		//var areas = playerArea.GetOverlappingAreas();
		//foreach (Area2D item in areas)
		//{
		//	if(item != null)
		//	{
		//		InventoryObject inv = item.GetParent() as InventoryObject;
		//		if(inv != null)
		//		{
		//			CollidingWithInvObject(inv);
		//		}
		//	}
		//}

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
			currentFacing = FacingDir.Left;
			movingDirection.x -= 1;
			//velocity += new Vector2(-horizontalMovementPower,0) * delta;
		}

		
		//Only if larger than unit vector than scale down MovingDirection
		if(movingDirection.LengthSquared() > movingDirection.Normalized().LengthSquared())
		{
			movingDirection = movingDirection.Normalized();
		}

		velocity = movingDirection * movementSpeed * movespeedScalar * delta;  

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
		
		//Allow the player to skip the last few frames to attack quickly again
		if(Godot.Input.IsActionPressed("PlayerAttack") && attacking == false)
		{

			//if currently playing then reset the animation cancel the rest of the anim
			if(weaponAnimPlayer.IsPlaying())
			{
				weaponAnimPlayer.Stop(true);
			}

			//animatedSprite.Play("Character Attack");

			Vector2 mousePos = GetGlobalMousePosition();
			//AngleToPoint does what we need, literally dont need to do anything else, sweet
			weaponSprite.Rotation = Position.AngleToPoint(mousePos) - Mathf.Pi/2.0f;
			weaponAnimPlayer.Play("BasicWeaponAttackAnim");
			attackingThisFrame = true;
		}

		//Don't move while attacking
		if(attacking || attackingThisFrame)
		{
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
				currentlyOverlappedOre.CreateInventoryObject();
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

		//velocity.y += gravity * delta;
		
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

		//Slow down movement and normalize evert frame
		movingDirection *= 0.6f;

		//Update velocity last
		velocity = MoveAndSlide(velocity, new Vector2(0,-1)) * 0.80f;
		attackingThisFrame = false;
  }
}
