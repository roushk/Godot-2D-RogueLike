using Godot;
using System;
using System.Collections.Generic;

public class BaseCraftingMaterials : Resource
{
  [Export]
  public string name { get; private set; } = "BaseCraftingMaterial";

  [Export]
  public int ingotCost { get; private set; } = 5;

  [Export]
  public Materials.MaterialType materialType { get; private set; } = Materials.MaterialType.Undefined;

  [Export]
  public Color tint = new Color(0,0,0,0);

  public TextureRect sprite = new TextureRect();

  [Export(PropertyHint.Enum)]
  public Dictionary<Parts.PartType,List<Materials.MaterialStatData>> materialProperties;
  
  public BaseCraftingMaterials()
  {
    materialProperties = new Dictionary<Parts.PartType,List<Materials.MaterialStatData>>();
  }

  public BaseCraftingMaterials(string _name, int _ingotCost, Materials.MaterialType _materialType, Color _tint) 
  {
    materialProperties = new Dictionary<Parts.PartType,List<Materials.MaterialStatData>>();
    
    name = _name;
    ingotCost = _ingotCost;
    materialType = materialType;
    tint = _tint;
  }
}

namespace Materials
{
  //Each material is linked to a tint
  public enum Material
  {
    Iron,       //Ore Chunks 0
    Copper,     //Ore Chunks 1
    Silver,     //Ore Chunks 2
    Gold,       //Ore Chunks 3
    Mithril,    //Ore Chunks 4
    Cobalt,     //Ore Chunks 6
    Bronze,     //Currently do everything before bronze
    Coal,       //Ore Chunks 5
    Tin,
    Steel,
    Platinum,
    Adamantite,
    Darksteel,
    Titanium,
    Undefined,
  }

  public enum MaterialType
  {
    Undefined,
    Metal,
    Wood,
    String,
  }

  public enum MaterialStatType
  {
    Undefined,
    Damage,
    CritChange,
    CritDamage,
    AttackSpeed,
    Health,
  }

  //Static class of colors tints
  public static class MaterialTints
  {
    public static Dictionary<Materials.Material, Color> tints = new Dictionary<Materials.Material, Color>();

    static MaterialTints()
    {

      tints[Materials.Material.Iron] =        new Color("e8e8e8");
      tints[Materials.Material.Silver] =      new Color("e6f2ff");  //TODO update silver so its not just platinum
      tints[Materials.Material.Coal] =        new Color("404040");
      tints[Materials.Material.Copper] =      new Color("e8a25d");
      tints[Materials.Material.Tin] =         new Color("faf4dc");
      tints[Materials.Material.Bronze] =      new Color("e8c774");
      tints[Materials.Material.Steel] =       new Color("a2e8b7");
      tints[Materials.Material.Gold] =        new Color("e8dc5d");
      tints[Materials.Material.Platinum] =    new Color("e6f2ff");
      tints[Materials.Material.Adamantite] =  new Color("e86868");
      tints[Materials.Material.Mithril] =     new Color("a2e8b7");
      tints[Materials.Material.Cobalt] =      new Color("a2aee8");
      tints[Materials.Material.Darksteel] =   new Color("696969");
      tints[Materials.Material.Titanium] =    new Color("ffffff");
      tints[Materials.Material.Undefined] =   new Color(1,1,1,0.5f); //transparent 50%, also ffffff7e doesnt work??
      
    }
  }

  //Material Stat Data Piece
  public class MaterialStatData : Resource
  {
    public MaterialStatData(MaterialStatType _statType, int _statData){statType = _statType;statData = _statData;}

    [Export]
    public MaterialStatType statType = MaterialStatType.Undefined;

    [Export]
    public int statData = 0;
  }
}

//TODO Move this to a PiecesEnum.cs and generate that code from a list of .png files inside of the My_Art/Parts folder 
namespace Parts
{
  public static class PartTypeConversion
  {
    public static PartType FromString(string input)
    {
      
      foreach (Parts.PartType type in Enum.GetValues(typeof(Parts.PartType)))  
      {
        if(input == type.ToString())
          return type;   
      }
      throw(new Exception("Enum " + input + " Does not exist in Parts.PartType."));

      //redundent return statement
      //return PartType.Undefined;
    }

    public static int CompareParts(Parts.PartBlueprint x, Parts.PartBlueprint y)
    {
      //Sort parts according to the
      return x.partType - y.partType;
    }
  }



  //Going to use enums to simplify the number of classes + adding more types of pieces can be data read into the piece data type instead of some RTTI/CTTI
  public enum PartType : int
  {
    Undefined,
    Blade,     //Blades, mace heads, tool heads, etc
    Guard,
    Handle,
    Pommel,
    
  //  HammerHead,   //Art Todo
  //  BattleaxeHead,  //Art Todo
  //  LargeGuard,   //Art Todo
  //  MediumHandle,   //Art Todo
  //  Medium_Mace,
  }
  //Pickaxe is {LargeHandle, ToolBinding, PickaxeHead}
  //Axe is {LargeHandle, ToolBinding, AxeHead}
  //Dagger is {Pommel, Small Handle, Small Guard, Small Blade}
  //Sword is a {Pommel, Small Handle, Medium Guard, Medium Blade}

}


