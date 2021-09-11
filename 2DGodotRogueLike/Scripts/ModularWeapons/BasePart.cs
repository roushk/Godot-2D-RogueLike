using Godot;
using System;
namespace Parts
{
  public delegate void WeaponBPNodeFunc(WeaponBlueprintNode node);
  //Node structure for weapon blueprints to represent the current parts as a node structure for positioning objects
  public class WeaponBlueprintNode
  {
    public PartBlueprint part = null;
    public WeaponBlueprintNode parent = null;
    public Vector2 currentOffset = Vector2.Zero;
    public System.Collections.Generic.Dictionary<AttachPoint,WeaponBlueprintNode> children = new System.Collections.Generic.Dictionary<AttachPoint,WeaponBlueprintNode>();

    public WeaponBlueprintNode(){}
    public WeaponBlueprintNode(PartBlueprint _part, WeaponBlueprintNode _parent)
    {
      part = _part;
      parent = _parent;
    }

    public void IterateNode(WeaponBPNodeFunc iterFunc)
    {
      iterFunc(this);
      foreach (var child in children)
      {
        child.Value.IterateNode(iterFunc);
      }
    }

    public void ClearNodeChildren()
    {
      foreach (var child in children)
      {
        child.Value.ClearNodeChildren();
      }
      children.Clear();
    }
  }

  public class AttachPoint
  {
    public Vector2 pos = new Vector2();
    public Godot.Collections.Array<Parts.PartType> partTypes = new  Godot.Collections.Array<Parts.PartType>();
    public bool attachedPart = false;
    public AttachPoint(Vector2 _pos, Godot.Collections.Array<Parts.PartType> _partTypes)
    {
      pos = _pos;
      partTypes = _partTypes;
    }
  }

  public class PartStats 
  {
    public int baseSlashDamage = 100;
    public int baseStabDamage = 100;
    public int baseAttackSpeed = 100;
    public int baseSwingSpeed = 100;
    public int baseLength = 100;
    public string specialStat = "None";

    //Sets special stat to both
    public static PartStats GetCombinationOfStats(PartStats lhs, PartStats rhs)
    {
      PartStats result = new PartStats();
      //-200 cause base is 100 and we want the extras of each 
      result.baseSlashDamage += lhs.baseSlashDamage + rhs.baseSlashDamage - 200;
      result.baseStabDamage += lhs.baseStabDamage + rhs.baseStabDamage - 200;
      result.baseAttackSpeed += lhs.baseAttackSpeed + rhs.baseAttackSpeed - 200;
      result.baseSwingSpeed += lhs.baseSwingSpeed + rhs.baseSwingSpeed - 200;
      result.baseLength += lhs.baseLength + rhs.baseLength - 200;

      if(lhs.specialStat != "None" && rhs.specialStat != "None")
        result.specialStat = lhs.specialStat + " and " + rhs.specialStat;
      else if(lhs.specialStat != "None")
        result.specialStat = lhs.specialStat;
      else if(rhs.specialStat != "None")
        result.specialStat = rhs.specialStat;

      return result;
    }

    [Export]
    static public Color negativeStatColor = new Color("bd1919");
    [Export]
    static public Color positiveStatColor = new Color("3fc41a");
    [Export]
    static public Color specialStatColor = new Color("bd20b2");
    [Export]
    static public Color normalstatcolor = new Color(1,1,1,1);
    // returns a string of a - b, 100 - 20 returns "+80) and empty str for zero

    public string BBCodeColorString(string str, Color color)
    {
      return "[color=#" + color.ToHtml(false) + "]" + str + "[/color]";
    }

    public string GetSignAndValue(int a, int b)
    {
      string str = "";
      if(a - b > 0)
      {
        //if color text then set the color of the text, if not then use the normal color
        str = BBCodeColorString(" + " + Mathf.Abs(a - b), positiveStatColor);
      }
      else if(a - b < 0)
      {
        //if color text then set the color of the text, if not then use the normal color
        str = BBCodeColorString(" - " + Mathf.Abs(a - b), negativeStatColor);
      }
      else
        str = BBCodeColorString(" + 0 ", normalstatcolor);

      return str;
    }

    string GenerateSingleStatText(string name, int value, int threshold, bool relativeNum = true)
    {
      string baseStat = "";
      //if(value != threshold)
      //{
        baseStat = name + (relativeNum ? GetSignAndValue(value, threshold) : " " + value.ToString()) + "\n";
      //}
      return baseStat;
    }
    //Generates text of the stats
    public string GenerateStatText(PartBlueprint oldPart = null, int threshold = 100, bool relativeNum = true)
    {
      int oldPartSlashDamage = 0;
      int oldPartStabDamage = 0;
      int oldPartAttackSpeed = 0;
      int oldPartSwingSpeed = 0;
      int oldPartLength = 0;
      if(oldPart != null)
      {
        oldPartSlashDamage = oldPart.stats.baseSlashDamage;
        oldPartStabDamage = oldPart.stats.baseStabDamage;
        oldPartAttackSpeed = oldPart.stats.baseAttackSpeed;
        oldPartSwingSpeed = oldPart.stats.baseSwingSpeed;
        oldPartLength = oldPart.stats.baseLength;
        threshold = 0;
      }
      
      string baseSlashStat =      GenerateSingleStatText("Slash Damage", baseSlashDamage - oldPartSlashDamage, threshold, relativeNum);
      string baseStabStat =       GenerateSingleStatText("Stab Damage", baseStabDamage - oldPartStabDamage, threshold, relativeNum);
      string baseAttackSpeedStat =  GenerateSingleStatText("Attack Speed", baseAttackSpeed - oldPartAttackSpeed, threshold, relativeNum);
      string baseSwingStat =      GenerateSingleStatText("Swing Speed", baseSwingSpeed - oldPartSwingSpeed, threshold, relativeNum);
      string baseLengthStat =     GenerateSingleStatText("Length", baseLength - oldPartLength, threshold, relativeNum);

      string specialStatText = "";
      if(specialStat != "None")
        specialStatText = "Special: " + BBCodeColorString(specialStat, specialStatColor) + "\n";
      
      //Ternary to return stats if they exist or "No Stat Changes" if no stat changes
      string tempStr = baseSlashStat + baseStabStat + baseAttackSpeedStat + baseSwingStat + baseLengthStat + specialStatText;
      
      //remove the last newline
      if(tempStr != "")
      {
        tempStr = tempStr.Remove(tempStr.FindLast("\n"),1);
      }
      else
      {
        tempStr = "No Stat Changes";
      }
      
      return tempStr;
    }
  }

  public class PartBlueprint : Resource
  {
    public static long currentUniquePieceNum = 0;

    //UUID for pieces
    public long uuid { get; private set; } = currentUniquePieceNum++;

    [Export]
    public string name { get; set; } = "BasePiece";

    [Export]
    public int materialCost { get; set; } = 5;

    [Export]
    public PartType partType { get; set; } = PartType.Undefined;
    
    public Texture texture { get; set; }
    public BitMap bitMask { get; set; }
    
    public PartStats stats = new PartStats();
    public Vector2 baseAttachPoint = new Vector2();
    
    //List of tuples of x/y coords and arrays of part types that are accepted
    public System.Collections.Generic.List<AttachPoint> partAttachPoints = 
      new System.Collections.Generic.List<AttachPoint>();
    
    public PartBlueprint(){}
    public PartBlueprint(PartBlueprint rhs)
    {
      name = rhs.name;
      materialCost = rhs.materialCost;
      partType = rhs.partType;
      texture = rhs.texture;
      bitMask = rhs.bitMask;
    }
  }
  public class PartConstructed : PartBlueprint
  {
    [Export]
    public Materials.MaterialType materialType { get; private set; } = Materials.MaterialType.Undefined;
    public PartConstructed() : base(){}
  }
}
