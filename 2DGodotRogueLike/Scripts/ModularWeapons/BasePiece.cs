using Godot;
using System;
namespace Pieces
{
    public class BasePiece : Resource
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
        public PieceType pieceType { get; private set; } = PieceType.Undefined;
        
        [Export]
        public string spritePath {get; private set; }
        
        public Sprite sprite { get; private set; }

        public BasePiece(){}
        public BasePiece(string _name, PieceType _pieceType, int _materialCost, Materials.MaterialType _materialType) 
        {
            name = _name;
            pieceType = _pieceType;
            materialCost = _materialCost;
            materialType = _materialType;
        }
    }
}
