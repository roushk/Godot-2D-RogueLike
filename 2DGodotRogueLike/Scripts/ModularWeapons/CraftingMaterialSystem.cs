using Godot;
using Godot.Collections;
using System;

public class MaterialAssetSystem : Node
{
    //Dict of material tints to lookup of pieces
    Dictionary<Materials.Material, Color> materialTints = new Dictionary<Materials.Material, Color>();

    //Dict of type to list of pieces
    Dictionary<Pieces.PieceType, Array<Pieces.Piece>> pieces = new Dictionary<Pieces.PieceType, Array<Pieces.Piece>>();
    
    public override void _Ready()
    {
        //https://www.c-sharpcorner.com/article/loop-through-enum-values-in-c-sharp/
        //For each Piece type generate an array in the pieces dict
        foreach (Pieces.PieceType type in Enum.GetValues(typeof(Pieces.PieceType)))  
        { 
            pieces[type] = new Array<Pieces.Piece>();
        }

        Color ironBaseColor = new Color("e8e8e8");
        //materialTints = data from file
        //Pieces = data from file
        //genuinely using hex str to int lmaoo
        materialTints[Materials.Material.Copper] =      ironBaseColor - new Color("e8a25d");
        materialTints[Materials.Material.Tin] =         ironBaseColor - new Color("faf4dc");
        materialTints[Materials.Material.Bronze] =      ironBaseColor - new Color("e8c774");
        materialTints[Materials.Material.Steel] =       ironBaseColor - new Color("a2e8b7");
        materialTints[Materials.Material.Gold] =        ironBaseColor - new Color("e8dc5d");
        materialTints[Materials.Material.Platinum] =    ironBaseColor - new Color("e6f2ff");
        materialTints[Materials.Material.Adamantite] =  ironBaseColor - new Color("e86868");
        materialTints[Materials.Material.Mithril] =     ironBaseColor - new Color("a2e8b7");
        materialTints[Materials.Material.Cobalt] =      ironBaseColor - new Color("a2aee8");
        materialTints[Materials.Material.Darksteel] =   ironBaseColor - new Color("696969");
        materialTints[Materials.Material.Titanium] =    ironBaseColor - new Color("ffffff");
    }

    public override void _Process(float delta)
    {

    }
}
