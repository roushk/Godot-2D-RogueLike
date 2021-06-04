using Godot;
using System;
namespace Parts
{
    public class PartStats 
    {
        public int baseSlashDamage = 80;
        public int baseStabDamage = 80;
        public int baseAttackSpeed = 60;
        public int baseSwingSpeed = 80;
        public int baseLength = 200;
        public string specialStat = "None";
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

        public PartBlueprint(){}
        public PartBlueprint(PartBlueprint rhs)
        {
            name = rhs.name;
            materialCost = rhs.materialCost;
            partType = rhs.partType;
            texture = rhs.texture;
            bitMask = rhs.bitMask;
        }

        [Export]
        public Color negativeStatColor = new Color("bd1919");
        [Export]
        public Color positiveStatColor = new Color("3fc41a");
        [Export]
        public Color specialStatColor = new Color("bd20b2");
        [Export]
        public Color normalstatcolor = new Color(1,1,1,1);
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
                str = BBCodeColorString(" + " + Mathf.Abs(a - b), positiveStatColor);
            }
            else if(a - b < 0)
            {
                str = BBCodeColorString(" - " + Mathf.Abs(a - b), negativeStatColor);
            }
            return str;
        }

        //Generates text of the stats
        public string GenerateStatText()
        {
            string baseSlashStat = "";
           
            if(stats.baseSlashDamage != 100)
            {
                baseSlashStat = "Slash Damage" + GetSignAndValue(stats.baseSlashDamage, 100) + "\n";
            }

            string  baseStabStat = "";
            if(stats.baseStabDamage != 100)
            {
                baseStabStat = "Stab Damage" + GetSignAndValue(stats.baseStabDamage, 100) + "\n";
            }

            string baseAttackSpeedStat = "";
            if(stats.baseAttackSpeed != 100)
            {
                baseAttackSpeedStat = "Attack Speed" + GetSignAndValue(stats.baseAttackSpeed, 100) + "\n";
            }

            string baseSwingStat = "";
            if(stats.baseSwingSpeed != 100)
            {
                baseSwingStat = "Swing Speed"+ GetSignAndValue(stats.baseSwingSpeed, 100) + "\n";
            }

            string baseLengthStat = "";
            if(stats.baseLength != 100)
            {
                baseLengthStat = "Weapon Length" + GetSignAndValue(stats.baseLength, 100) + "\n";
            }

            string specialStatText = "";
            if(stats.specialStat != "None")
                specialStatText = "Special: " + BBCodeColorString(stats.specialStat, specialStatColor) + "\n";
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
    public class PartConstructed : PartBlueprint
    {
        [Export]
        public Materials.MaterialType materialType { get; private set; } = Materials.MaterialType.Undefined;
        public PartConstructed() : base(){}
    }
}
