using Godot;
using System;

public class PartResource : Resource
{
    [Export]
    public string partName = "Part Name";
    [Export]
    public string partTextureDir = "res://Assets/Art/My_Art/Parts/Medium_Blade_PartName.png";
    [Export]
    public Parts.PartType partType = Parts.PartType.Medium_Blade;
    [Export]
    public int partCost = 5;

    public PartResource(){}
    
}
