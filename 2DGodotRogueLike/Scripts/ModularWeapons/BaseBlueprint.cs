using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public class BaseBlueprint 
{
    public string name { get; private set; } = "BaseBlueprint";

    //A blueprint is made up of a list of required pieces
    public Array<Pieces.PieceType> requiredPieces = new Array<Pieces.PieceType> {Pieces.PieceType.WeaponPommel, Pieces.PieceType.WeaponHandle, Pieces.PieceType.SwordBlade, Pieces.PieceType.SwordGuard};

    //Returns bool on if its craftable and if not returns a list of piece types that are needed
    public Tuple<bool,Array<Pieces.PieceType>> IsCraftableWithGivenMaterials(Array<Pieces.Piece> materials)
    {
        //create copies
        List<long> usedMaterials = new List<long>();

        //List of missing pieces
        Array<Pieces.PieceType> missingPieces = new Array<Pieces.PieceType>();
        
        //For every piece
        foreach (var requiredPiece in requiredPieces)
        {
            bool foundMaterial = false;
            //check through the existing materials
            foreach (var existingMaterial in materials)
            {
                //If we have the correct piece type and its not contained within the already used materials
                if(existingMaterial.pieceType == requiredPiece && !usedMaterials.Contains(existingMaterial.uuid))
                {
                    //add it to the list of used materials and break searching for the current piece
                    usedMaterials.Add(existingMaterial.uuid);
                    foundMaterial = true;
                    break;
                }
            }

            //If we get to the end of the materials and we have not found a material then add a missing material to the list
            if(foundMaterial == false)
            {
                missingPieces.Add(requiredPiece);
            }
        }
        
        //Return both the bool on if craftable and the list of pieces that are needed
        return new Tuple<bool,Array<Pieces.PieceType>>(missingPieces.Count == 0, missingPieces);
    }
}
