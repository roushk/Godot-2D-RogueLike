using Godot;
using System;
namespace Parts
{
    public class BasePart : Resource
    {
        public static long currentUniquePieceNum = 0;

        //UUID for pieces
        public long uuid { get; private set; } = currentUniquePieceNum++;

        [Export]

        public string name { get; private set; } = "BasePiece";
        [Export]
        public int materialCost { get; private set; } = 5;

        [Export]
        public Materials.MaterialType materialType { get; private set; } = Materials.MaterialType.Undefined;

        [Export]
        public PartType pieceType { get; private set; } = PartType.Undefined;
        
        [Export]
        public string spritePath {get; private set; }
        
        public Sprite sprite { get; private set; }

        public BasePart(){}
        public BasePart(string _name, PartType _pieceType, int _materialCost, Materials.MaterialType _materialType) 
        {
            name = _name;
            pieceType = _pieceType;
            materialCost = _materialCost;
            materialType = _materialType;
        }
    }
}
