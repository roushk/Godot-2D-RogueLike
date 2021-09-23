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
    //Slash damage is damage in slashing attack
    public float slashDamage = 0;
    //Stab damage is damage in stab attack
    public float stabDamage = 0;
    //Attack speed is howhow many attacks per second
    public float attackWindUp = 0;
    //Swing speed affects how fast the blade is swung, more is fast swing and stab
    public float attackWindDown = 0;
    //Length is the reach of the weapon
    public float length = 0;
    //Special stat is a special stat
    public string specialStat = "None";

    public PartStats(){}
    public PartStats(float slash, float stab, float windup, float winddown, float _length)
    {
      slashDamage = slash;
      stabDamage = stab;
      attackWindUp = windup;
      attackWindDown = winddown;
      length = _length;
    }

    //Sets special stat to both
    public static PartStats GetCombinationOfStats(PartStats lhs, PartStats rhs, Materials.Material material)
    {
      PartStats result = new PartStats();
      result.slashDamage += (lhs.slashDamage + rhs.slashDamage) * Materials.MaterialStats.stats[material].damageMult;
      result.stabDamage += (lhs.stabDamage + rhs.stabDamage) * Materials.MaterialStats.stats[material].damageMult;
      result.attackWindUp += (lhs.attackWindUp + rhs.attackWindUp) * Materials.MaterialStats.stats[material].windMult;
      result.attackWindDown += (lhs.attackWindDown + rhs.attackWindDown) * Materials.MaterialStats.stats[material].windMult;
      result.length += lhs.length + rhs.length;

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

    public string GetSignAndValue(float a, float b, bool lowNumberGreen = true)
    {
      string str = "";
      if(a - b > 0)
      {
        //if color text then set the color of the text, if not then use the normal color
        str = BBCodeColorString(" + " + Mathf.Abs(a - b), lowNumberGreen?positiveStatColor:negativeStatColor);
      }
      else if(a - b < 0)
      {
        //if color text then set the color of the text, if not then use the normal color
        str = BBCodeColorString(" - " + Mathf.Abs(a - b), lowNumberGreen?negativeStatColor:positiveStatColor);
      }
      else
        str = BBCodeColorString(" + 0 ", normalstatcolor);

      return str;
    }

    string GenerateSingleStatText(string name, float value, float threshold, bool relativeNum = true, bool lowNumberGreen = true)
    {
      string baseStat = "";
      //if(value != threshold)
      //{
        baseStat = name + (relativeNum ? GetSignAndValue(value, threshold, lowNumberGreen) : " " + value.ToString()) + "\n";
      //}
      return baseStat;
    }
    //Generates text of the stats
    public string GenerateStatText(PartBlueprint oldPart = null, float threshold = 0, bool relativeNum = true)
    {
      float oldPartSlashDamage = 0;
      float oldPartStabDamage = 0;
      float oldPartWindUp = 0;
      float oldPartWindDown = 0;
      float oldPartLength = 0;
      if(oldPart != null)
      {
        oldPartSlashDamage = oldPart.stats.slashDamage;
        oldPartStabDamage = oldPart.stats.stabDamage;
        oldPartWindUp = oldPart.stats.attackWindUp;
        oldPartWindDown = oldPart.stats.attackWindDown;
        oldPartLength = oldPart.stats.length;
        threshold = 0;
      }
      
      string baseSlashStat =        GenerateSingleStatText("Slash Damage", slashDamage - oldPartSlashDamage, threshold, relativeNum);
      string baseStabStat =         GenerateSingleStatText("Stab Damage", stabDamage - oldPartStabDamage, threshold, relativeNum);
      string baseAttackSpeedStat =  GenerateSingleStatText("Wind Up", attackWindUp - oldPartWindUp, threshold, relativeNum, false);
      string baseSwingStat =        GenerateSingleStatText("Wind Down", attackWindDown - oldPartWindDown, threshold, relativeNum, false);
      string baseLengthStat =       GenerateSingleStatText("Length", length - oldPartLength, threshold, relativeNum);

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

    public Materials.Material currentMaterial = Materials.Material.Undefined;

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

    public void ResetPart()
    {
      currentMaterial = Materials.Material.Undefined;
    }
  }

  public class ConstructedWeapon
  {
    public PartStats stats = new PartStats();
    public Texture texture;
    public string detailText;
    public string name;
    public ConstructedWeapon(string _name,PartStats _stats, Texture _sprite, string _detailText)
    {
      name = _name;
      stats = _stats;
      texture = _sprite;
      detailText = _detailText;
    }
  }
}
