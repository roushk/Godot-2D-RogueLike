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
    }
    public class PartConstructed : PartBlueprint
    {
        [Export]
        public Materials.MaterialType materialType { get; private set; } = Materials.MaterialType.Undefined;
        public PartConstructed() : base(){}
    }
}
