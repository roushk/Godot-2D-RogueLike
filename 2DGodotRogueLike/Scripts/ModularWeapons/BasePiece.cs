using Godot;
using System;
namespace Pieces
{
    //Going to use enums to simplify the number of classes + adding more types of pieces can be data read into the piece data type instead of some RTTI/CTTI
    public enum PieceType : int
    {
        Undefined,
        ToolHandle,
        ToolCrossing,
        AxeHead,
        PickaxeHead,
        SwordBlade,
        SwordGuard,
        DaggerBlade,
        DaggerGuard,
        WeaponPommel,
        WeaponHandle
    }

    public class Piece
    {
        public static long currentUniquePieceNum = 0;

        //UUID for pieces
        public long uuid { get; private set; } = currentUniquePieceNum++;

        public string name { get; private set; } = "BasePiece";
        public int materialCost { get; private set; } = 5;
        public Materials.MaterialType materialType { get; private set; } = Materials.MaterialType.Undefined;
        public PieceType pieceType { get; private set; } = PieceType.Undefined;
        public Sprite sprite { get; private set; }

        public Piece(string _name, PieceType _pieceType, int _materialCost, Materials.MaterialType _materialType) 
        {
            name = _name;
            pieceType = _pieceType;
            materialCost = _materialCost;
            materialType = _materialType;
        }
    }
}
