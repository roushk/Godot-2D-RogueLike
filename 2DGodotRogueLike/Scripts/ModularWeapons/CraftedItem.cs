using Godot;
using System;
using Godot.Collections;

public class CraftedItem : Resource
{
    [Export]
    public string name { get; private set; } = "CraftedItem";
    
    [Export]
    public string iconSpriteName  { get; private set; } = "Medium_Sword";
    
    //A blueprint is made up of a list of required pieces
    public Array<Tuple<Materials.Material, Parts.PartBlueprint>> weaponPieces = new Array<Tuple<Materials.Material, Parts.PartBlueprint>>{};

    //Todo Generate sprite from the pieces, or maybe just generate a single button from them

}
