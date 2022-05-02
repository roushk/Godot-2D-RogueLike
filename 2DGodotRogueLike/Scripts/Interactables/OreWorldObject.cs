using Godot;
using System;

public class OreWorldObject : Interactable
{
  [Signal]
  public delegate void OreOverlappingSignal(bool overlapping, OreWorldObject oreNode);
  
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  // Called when the node enters the scene tree for the first time.
  [Export]
  public Materials.Material material;

  [Export]
  public int amountInOre = 1;

  [Export]
  public float timeToMine = 3;

  CPUParticles2D cpuParticles2D;
  
  private PackedScene inventoryObjectScene = (PackedScene)ResourceLoader.Load("res://Scenes/PickupWorldObjects/InventoryPickupWorldObject.tscn");

  public override void _Ready()
  {
    base._Ready();
    cpuParticles2D = GetNode("CPUParticles2D") as CPUParticles2D;

    animatedSprite.Animation = "Iron Ores";
    
    //Modulate the ore to the tint of the material
    //TODO unique sprites for each material type
    animatedSprite.Modulate = Materials.MaterialTints.tints[material];
    
    animatedSprite.Frame = random.Next(0,10);
  }

  public void CreateInventoryObject()
  {
    Spatial newInvObject = inventoryObjectScene.Instance() as Spatial; 
    GetTree().Root.AddChild(newInvObject); 

    //adds the new object as a child to the parent of this object so they are not tetherd together.
    
    newInvObject.Translation = this.Translation;
    var invObj = (newInvObject as InventoryPickupWorldObject);
    invObj.inventoryObjectName = material.ToString();
    invObj.material = material;
    invObj.numMaterials = amountInOre;
    invObj.isMaterial = true;

    //should be setting it to data loaded from a file for the craftable material property
    newInvObject.GetNode<AnimatedSprite>("AnimatedSprite").Animation = "Ore Chunks";
    newInvObject.GetNode<AnimatedSprite>("AnimatedSprite").Frame = (int)material;
    //InventoryObject.Get
  }

  public override void _PhysicsProcess(float delta)
  {
    base._PhysicsProcess(delta);

    //TODO move this elseware
		if(playerInteracting)
		{
			cpuParticles2D.Emitting = true;
			timeToMine -= delta;

			//if spent enough time mining ore
			if(timeToMine <= 0)
			{
				//Spawn ore item
				CreateInventoryObject();

        //Clean up interactables stuff
        ObjectBeingRemoved();

				//Destroy ore object
				QueueFree();

			}
		}
    else
    {
			cpuParticles2D.Emitting = false;
    }
  }
}
