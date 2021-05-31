using Godot;
using Godot.Collections;
using System;

public class CraftingMaterialSystem : Node
{
    //Signals

    public void SwitchToIronOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Iron];
    }

    public void SwitchToCopperOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Copper];
    }
    
    public void SwitchToTinOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Tin];
    }

    public void SwitchToAdamantiteOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Adamantite];
    }

    //Dict of material tints to lookup of pieces
    Dictionary<Materials.Material, Color> materialTints = new Dictionary<Materials.Material, Color>();

    //Dict of type to list of pieces
    Dictionary<Pieces.PieceType, Array<Pieces.BasePiece>> pieces = new Dictionary<Pieces.PieceType, Array<Pieces.BasePiece>>();
    
    TextureRect ingot;


    public override void _Ready()
    {
        ingot = GetNode("Ingot") as TextureRect;

        //https://www.c-sharpcorner.com/article/loop-through-enum-values-in-c-sharp/
        //For each Piece type generate an array in the pieces dict
        foreach (Pieces.PieceType type in Enum.GetValues(typeof(Pieces.PieceType)))  
        { 
            pieces[type] = new Array<Pieces.BasePiece>();
        }

        Color ironBaseColor = new Color("e8e8e8");
        //materialTints = data from file
        //Pieces = data from file
        //genuinely using hex str to int lmaoo
        materialTints[Materials.Material.Iron] =        new Color("e8e8e8");
        materialTints[Materials.Material.Copper] =      new Color("e8a25d");
        materialTints[Materials.Material.Tin] =         new Color("faf4dc");
        materialTints[Materials.Material.Bronze] =      new Color("e8c774");
        materialTints[Materials.Material.Steel] =       new Color("a2e8b7");
        materialTints[Materials.Material.Gold] =        new Color("e8dc5d");
        materialTints[Materials.Material.Platinum] =    new Color("e6f2ff");
        materialTints[Materials.Material.Adamantite] =  new Color("e86868");
        materialTints[Materials.Material.Mithril] =     new Color("a2e8b7");
        materialTints[Materials.Material.Cobalt] =      new Color("a2aee8");
        materialTints[Materials.Material.Darksteel] =   new Color("696969");
        materialTints[Materials.Material.Titanium] =    new Color("ffffff");
    }

    public override void _Process(float delta)
    {

    }
}
