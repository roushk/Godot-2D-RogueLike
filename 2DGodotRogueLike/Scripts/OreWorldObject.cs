using Godot;
using System;

public class OreWorldObject : Node2D
{
  [Signal]
  public delegate void OreOverlappingSignal(bool overlapping,OreWorldObject oreNode);
  
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  // Called when the node enters the scene tree for the first time.
  [Export]
  public string oreName;

  [Export]
  public int amountInOre = 1;

  [Export]
  public float timeToMine = 3;

  //GD.Load()
  
  private PackedScene _inventoryObjectScene = (PackedScene)GD.Load("res://InventoryObject.tscn");

  //const inventoryObject = Godot.ResourcePreloader. ("res://Bullet.tscn")
  public override void _Ready()
  {

  }

  public void CreateInventoryObject()
  {
    Node _inventoryObject = _inventoryObjectScene.Instance(); 
    //adds the new object as a child to the parent of this object so they are not tetherd together.
    this.GetParent().AddChild(_inventoryObject); 
    var inventoryObject =  _inventoryObject as InventoryObject;
    inventoryObject.Position = this.Position;
    inventoryObject.inventoryObjectName = oreName;
    //should be setting it to data loaded from a file for the craftable material property 
    inventoryObject.GetNode<AnimatedSprite>("AnimatedSprite").Animation = "Ore Chunks";
    //InventoryObject.Get

  }

  public override void _Process(float delta)
  {

    AnimatedSprite animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;

    Area2D area2D = GetNode("Area2D") as Area2D;

    CollisionShape2D collisionShape2D = GetNode("Area2D/CollisionShape2D") as CollisionShape2D;

    CPUParticles2D cpuParticles2D = GetNode("CPUParticles2D") as CPUParticles2D;


  }


}
