using Godot;
using System;
using System.Collections.Generic;

public class PlayerTopDown : CombatCharacter
{
  // X/Z movement vector
  Vector3 movingDirection = Vector3.Zero;

	//Scaled multiplier for movespeed to make base 100 a decent speed
	const float movespeedScalar = 5.0f;

	float dashSpeed = 15.0f;
  
  float idleEpsilon = 10;

  bool grounded = true;

	[Export]
  bool attacking = false;

	bool currentlyInteracting = false;
	
	//Bool to stop player movement if holding down/spamming the attack button as the anim player is updated after the player so the player can move slightly between each and don't want that
	bool attackingThisFrame = false;
	
	bool finishWinding = true;
	Timer windTimer;

  //todo Doesnt quite work, need better way to detect if above fallable block
  bool OnTile = false;
  // Called when the node enters the scene tree for the first time.

	Area playerArea;
	AnimationPlayer weaponAnimPlayer;
	RayCast raycast2D;
	Sprite weaponSprite;
	HealthBar healthBar;

	public InventoryUI playerInventoryUI;
	public EndOfLevelUI endOfLevelUI;
	public PlayerUI playerUI;
	public CraftingMaterialSystem playerCraftingUI;

	private CurrentlySelectedUI _currentlySelectedUI;
	protected PlayerManager playerManager;

	bool firstTimeInit = true;

  [Export]
	Curve weaponWindCurve = new Curve();

	public HashSet<Interactable> interactablesInRange = new HashSet<Interactable>();

	public Interactable closestInteractable = null;

	//save incase the player walks away somehow
	public Interactable currentlyInteractingWith = null;
	

	//TODO change to crafted/selected weapon
	public Parts.PartStats weapon = new Parts.PartStats();

	float playerBaseKnockBack = 100.0f;
	float playerWeaponKnockback = 1.0f;

	string nextAnim = "SlashAttack";

	//overload get/set instead of function call
	public CurrentlySelectedUI currentlySelectedUI 
	{
		get{return _currentlySelectedUI;}
	
		set
		{
			playerUI.Visible = false;
			playerCraftingUI.Visible = false;
			playerInventoryUI.Visible = false;
			endOfLevelUI.Visible = false;

			if(value == CurrentlySelectedUI.None)
			{
				playerUI.Visible = true;
			}
			else if(value == CurrentlySelectedUI.CraftingScreen)
			{
				playerCraftingUI.Visible = true;
			}
			else if(value == CurrentlySelectedUI.InventoryScreen)
			{
				playerInventoryUI.Visible = true;
			}
			else if(value == CurrentlySelectedUI.EndLevelUI)
			{
				endOfLevelUI.Visible = true;
			}
			_currentlySelectedUI = value;
		}
	}

	public enum CurrentlySelectedUI
	{
		None,
		CraftingScreen,
		InventoryScreen,
		EndLevelUI
	}

	public override void CharacterDeadCallback(float damageTakenThatKilled)
  {
		//Play character death animation
		currentlySelectedUI = CurrentlySelectedUI.EndLevelUI;
		endOfLevelUI.resetPlayerOnContinue = true;
    //TODO play actual death animation here
		//MapManager.RemoveEntities;
    //this.QueueFree();
  }

	public void SetCurrentWeapon(Parts.ConstructedWeapon _weapon)
	{
		weapon = _weapon.stats;
		
	}


// Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
		base._Ready();
		characterType = CombatCharacter.CharacterType.Player;
		windTimer = GetNode<Timer>("WindTimer");
		playerArea = GetNode<Area>("PlayerInteractionArea3D");
		animatedSprite = GetNode<AnimatedSprite>("AnimatedSprite");
		weaponAnimPlayer = GetNode<AnimationPlayer>("WeaponSprite/WeaponAnimPlayer");
		raycast2D = GetNode<RayCast>("RayCast");
		weaponSprite = GetNode<Sprite>("WeaponSprite");

		Node camera = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode").playerCamera;
		
		//Link UI's
		healthBar = camera.GetNode<HealthBar>("PlayerUI/HealthBar");
		playerInventoryUI = camera.GetNode<InventoryUI>("PlayerInventoryUI");
		playerCraftingUI = camera.GetNode<CraftingMaterialSystem>("CraftingScreen");
		playerUI = camera.GetNode<PlayerUI>("PlayerUI");
		endOfLevelUI = camera.GetNode<EndOfLevelUI>("EndLevelUI");
		currentlySelectedUI = CurrentlySelectedUI.None;

		//Reset attacking sprite
		weaponAnimPlayer.Stop(true);
		attacking = false;

		//TODO set default weapon
		//weapon = 

		playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");


		weapon.slashDamage = 10;
		weapon.stabDamage = 10;
		weapon.attackWindUp = 10;
		weapon.attackWindDown = 10;
		weapon.length = 100;

  }

	void CollidingWithInvObject(InventoryPickupWorldObject inv)
	{
		Console.WriteLine("Overlapping Inventory Object" + inv.ToString());
		//Add to relevent material
		if(inv.isMaterial)
		{
			playerManager.playerInventory.AddMaterial(inv.material, inv.numMaterials);
		}
		else
		{
			playerManager.playerInventory.AddUniqueItem(inv.inventoryObjectName, inv.weapon);
		}

		//TODO setup callback if needed
		//inv.PickedUpCallback();
		//delete the object after adding it
		inv.QueueFree();
	}

	public void _on_PlayerInteractionArea_body_entered(Node body)
  {
		InventoryPickupWorldObject inv = body.GetParent() as InventoryPickupWorldObject;
		if(inv != null)
		{
			CollidingWithInvObject(inv);
			return;
		}

	}

	public void _on_PlayerInteractionArea_body_exited(Node body)
	{

	}

	//When the punch area overlaps
  public void _on_SlashArea_body_entered(Node body)
  {
    CombatCharacter character = body as CombatCharacter;
    if(character != null && character.characterType == CombatCharacter.CharacterType.Enemy)
    {
			character.DamageCharacter(weapon.slashDamage, (character.GlobalTransform.origin - GlobalTransform.origin).Normalized() * baseKnockBack * extraKnockback);
    }
  }

	//When the punch area overlaps
  public void _on_StabArea_body_entered(Node body)
  {
    CombatCharacter character = body as CombatCharacter;
    if(character != null && character.characterType == CombatCharacter.CharacterType.Enemy)
    {
			character.DamageCharacter(weapon.stabDamage, (character.GlobalTransform.origin - GlobalTransform.origin).Normalized() * baseKnockBack * extraKnockback);
    }
  }

  public void _on_PlayerInteractionArea_area_entered(Area body)
  {
		InventoryPickupWorldObject inv = body.GetParent() as InventoryPickupWorldObject;
		if(inv != null)
		{
			CollidingWithInvObject(inv);
		}
  }

  public void _on_PlayerInteractionArea_area_exited(Area body)
  {

  }

  /*
  public override void _Draw()
  {
	  this.DrawLine(Position,Position + new Vector2(velocity.x, velocity.y),Color.Color8(1,0,0,1));
	  this.DrawLine(Position,Position + new Vector2(0,50),Color.Color8(0,1,0,1));
  }
  */

	public void onTimerWindTimeOut()
	{
		weaponAnimPlayer.Play(nextAnim);
		finishWinding = true;
	}

	public void StartedAttacking()
	{
		//pause animation
		//wait for timer to callback continue animation
		weaponAnimPlayer.Stop();
		windTimer.WaitTime = Mathf.Max(weaponWindCurve.Interpolate(weapon.attackWindUp/1000.0f),0.001f);
		nextAnim = "SlashAttack";
		windTimer.Start();
		finishWinding = false;
	}

	public void EndedAttacking()
	{
		weaponAnimPlayer.Stop();
		windTimer.WaitTime = Mathf.Max(weaponWindCurve.Interpolate(weapon.attackWindDown/1000.0f),0.001f);
		nextAnim = "SlashWindDown";
		windTimer.Start();
		finishWinding = false;
	}

  //  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
		if(firstTimeInit)
		{
			firstTimeInit = false;
		}
		
		base._PhysicsProcess(delta);

		movingDirection = Vector3.Zero;
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
		if(!firstTimeInit)
			healthBar.SetHealth(currentHealth);

		Interactable closest = null;
		float closestInteractableDistance = float.MaxValue;

		interactablesInRange.RemoveWhere(node => !IsInstanceValid(node));

		foreach(var inter in interactablesInRange)
		{
      float distanceToPlayerSquared = GlobalTransform.origin.DistanceSquaredTo(inter.Translation);
			if(distanceToPlayerSquared < closestInteractableDistance)
			{
				closestInteractableDistance = distanceToPlayerSquared;
				closest = inter;
			}
			//inter.Modulate = new Color(0.5f,1,0.5f,1);
		}

		//if(closest != null)	
		//	closest.Modulate = new Color(0.5f,0.5f,1,1);

		closestInteractable = closest;

		raycast2D.CastTo = new Vector3(0,50,0);

		//the raycast only collides with the second layer so only the floors
		OnTile = raycast2D.IsColliding();

		//update player movement
		if(Godot.Input.IsActionPressed("PlayerUp"))
		{
			movingDirection.z -= 1;
		}
		if(Godot.Input.IsActionPressed("PlayerDown"))
		{
			movingDirection.z += 1;
		}
		if(Godot.Input.IsActionPressed("PlayerRight"))
		{
			//Prioritize left and right attacks more than up and down
			movingDirection.x += 1;
		}
		if(Godot.Input.IsActionPressed("PlayerLeft"))
		{
			movingDirection.x -= 1;
		}

		if(Godot.Input.IsActionJustPressed("PlayerInteract"))
		{
			if(closestInteractable != null)
			{
				currentlyInteractingWith = closestInteractable;
				currentlyInteractingWith.StartInteract();
			}
		}

		if(Godot.Input.IsActionPressed("PlayerInteract"))
		{
			//Deal with the cast that the object we were interacting with is destroyed (i.e. ore mined) then find new one
			if(closestInteractable != null && currentlyInteractingWith == null)
			{
				currentlyInteractingWith = closestInteractable;
				currentlyInteractingWith.StartInteract();
			}
		}

		if(Godot.Input.IsActionJustReleased("PlayerInteract"))
		{
			if(currentlyInteractingWith != null)
			{
				currentlyInteractingWith.EndInteract();
				if(currentlyInteractingWith.playerInteracting == false)
					currentlyInteractingWith = null;
			}
		}

		if(Godot.Input.IsActionJustReleased("PlayerInventory") && currentlySelectedUI == CurrentlySelectedUI.InventoryScreen)
		{
			//Pause stuff somehow
			currentlySelectedUI = CurrentlySelectedUI.None;
		}
		else if(Godot.Input.IsActionJustReleased("PlayerInventory") && currentlySelectedUI == CurrentlySelectedUI.None)
		{
			//Pause stuff somehow
			currentlySelectedUI = CurrentlySelectedUI.InventoryScreen;
		}

		//Set the currently interacting based on what the interator thinks and if not null
		currentlyInteracting = currentlyInteractingWith != null && currentlyInteractingWith.playerInteracting;

		//if no UI selected then player is visible
		if(currentlySelectedUI == CurrentlySelectedUI.None)
		{
			GetTree().Paused = false;
			animatedSprite.Visible = true;
		}
		else
		{
			//Pause game
			GetTree().Paused = true;
			animatedSprite.Visible = false;
		}
				
		//Attacking
		
		//Allow the player to skip the last few frames to attack quickly again and player is not in a UI
		if(Godot.Input.IsActionPressed("PlayerAttack") && attacking == false && currentlySelectedUI == CurrentlySelectedUI.None)
		{

			//if currently playing then reset the animation cancel the rest of the anim
			if(weaponAnimPlayer.IsPlaying())
			{
				weaponAnimPlayer.Stop(true);
			}

			//animatedSprite.Play("Character Attack");

		  //Vector2 mousePos = GetGlobalMousePosition();
		  //AngleToPoint does what we need, literally dont need to do anything else, sweet
			//weaponSprite.Rotation = Position.AngleToPoint(mousePos) - Mathf.Pi/2.0f;
			//TODO Actual raycast into the scene on ground plane to get cursor location for look at vec
			
			weaponSprite.Rotation = 0;
			weaponAnimPlayer.Play("SlashWindUp");
			weaponSprite.Scale = new Vector2(weapon.length/100.0f,weapon.length/100.0f);
			attackingThisFrame = true;
		}

		//Don't move while attacking or interacting with objects or in UI
		if(attacking || attackingThisFrame || currentlyInteracting || currentlySelectedUI != CurrentlySelectedUI.None)
		{
			movingDirection = Vector3.Zero;
		}

		velocity += movingDirection.Normalized() * movementSpeed * movespeedScalar * delta;  

		//if velocity x == 0 then dont change
		if(velocity.x > 0)
		{
			animatedSprite.FlipH = false;
		}
		else if(velocity.x < 0)
		{
			animatedSprite.FlipH = true;
		}


		//velocity.y += gravity * delta;
		
		//idle - if grounded and slow
		if(grounded && !attacking)
		{
			if((Mathf.Abs(velocity.x) > idleEpsilon || Mathf.Abs(velocity.y) > idleEpsilon))
			{
				//If more X than Y
				if(Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
				{
					animatedSprite.Play("Sideways_Walking");
				}
				else	//More Y Than X
				{
					//TODO once added walking up then swap depending on the dir
					animatedSprite.Play("Down_Walking");
				}
			}
			else
			{
				if(Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
				{
					animatedSprite.Play("Sideways_Idle");
				}
				else	//More Y Than X
				{
					//TODO once added idle up then swap depending on the dir
					animatedSprite.Play("Down_Idle");
				}
			}
		}

		
		//Player Dash, only resolve if can move
		if(Godot.Input.IsActionJustPressed("PlayerMovementAbility") && velocity.LengthSquared() != 0)
		{
			velocity = dashSpeed * movementSpeed * movespeedScalar * delta * movingDirection.Normalized();

			rollInvincibilityTimeLeft = rollMaxInvincibilityTimeLeft;
			//Play dash animation
			//Stop change of movement direction for a bit?
		}

		//Slow down movement and normalize evert frame
		

		//Update velocity last
		velocity = MoveAndSlide(velocity) * 0.6f;
		attackingThisFrame = false;
  }
}
