using Godot;
using System;
using System.Collections.Generic;

namespace Materials
{
    //Each material is linked to a tint
    public enum Material
    {
        Undefined,
        Iron,
        Bronze,
        Copper,
        Tin,
        Steel,
        Gold,
        Platinum,
        Adamantite,
        Mithril,
        Cobalt,
        Darksteel,
        Titanium,
    }

    public enum MaterialType
    {
        Undefined,
        Metal,
        Wood,
        String,
    } 

    public class BaseCraftingMaterial
    {
        public string name { get; private set; } = "BaseCraftingMaterial";
        public int ingotCost { get; private set; } = 5;
        public Materials.MaterialType materialType { get; private set; } = Materials.MaterialType.Undefined;
        public Material material { get; private set; } = Material.Undefined;
        public Color tint = new Color(0,0,0,0);

        public BaseCraftingMaterial(string _name, int _ingotCost, Materials.MaterialType _materialType, Materials.Material _material, Color _tint) 
        {
            name = _name;
            ingotCost = _ingotCost;
            materialType = materialType;
            material = _material;
            tint = _tint;
        }
    }
}

