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
  public Materials.Material material;

  [Export]
  public int amountInOre = 1;

  [Export]
  public float timeToMine = 3;

  AnimatedSprite animatedSprite;
  Area2D area2D;
  CollisionShape2D collisionShape2D;
  CPUParticles2D cpuParticles2D;
  //GD.Load()
  
  private PackedScene inventoryObjectScene = (PackedScene)ResourceLoader.Load("res://TemplateScenes/InventoryObject.tscn");

  //const inventoryObject = Godot.ResourcePreloader. ("res://Bullet.tscn")
  public override void _Ready()
  {
    animatedSprite = GetNode("AnimatedSprite") as AnimatedSprite;
    area2D = GetNode("Area2D") as Area2D;
    collisionShape2D = GetNode("Area2D/CollisionShape2D") as CollisionShape2D;
    cpuParticles2D = GetNode("CPUParticles2D") as CPUParticles2D;
  }

  public void CreateInventoryObject()
  {
    Node2D newInvObject = inventoryObjectScene.Instance() as Node2D; 
    GetTree().Root.AddChild(newInvObject); 

    //adds the new object as a child to the parent of this object so they are not tetherd together.
    
    newInvObject.GlobalPosition = this.GlobalPosition;
    var invObj = (newInvObject as InventoryObject);
    invObj.inventoryObjectName = material.ToString();
    invObj.material = material;
    invObj.numMaterials = amountInOre;
    invObj.isMaterial = true;

    //should be setting it to data loaded from a file for the craftable material property 
    newInvObject.GetNode<AnimatedSprite>("AnimatedSprite").Animation = "Ore Chunks";
    //InventoryObject.Get
  }

  public override void _Process(float delta)
  {

  }


}
