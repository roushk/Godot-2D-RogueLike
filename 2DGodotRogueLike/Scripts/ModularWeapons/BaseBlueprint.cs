using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

public class BaseBlueprint : Resource
{
    [Export]
    public string name { get; private set; } = "BaseBlueprint";

    //A blueprint is made up of a list of required pieces
    [Export]
    public Array<Pieces.PieceType> requiredPieces = new Array<Pieces.PieceType>();

    //Returns bool on if its craftable and if not returns a list of piece types that are needed
    public Tuple<bool,Array<Pieces.PieceType>> IsCraftableWithGivenMaterials(Array<Pieces.BasePiece> existingPieces)
    {
        //create copies
        List<long> usedPieces = new List<long>();

        //List of missing pieces
        Array<Pieces.PieceType> missingPieces = new Array<Pieces.PieceType>();
        
        //For every piece
        foreach (var requiredPiece in requiredPieces)
        {
            bool foundPiece = false;
            //check through the existing materials
            foreach (var existingPiece in existingPieces)
            {
                //If we have the correct piece type and its not contained within the already used materials
                if(existingPiece.pieceType == requiredPiece && !usedPieces.Contains(existingPiece.uuid))
                {
                    //add it to the list of used materials and break searching for the current piece
                    usedPieces.Add(existingPiece.uuid);
                    foundPiece = true;
                    break;
                }
            }

            //If we get to the end of the materials and we have not found a material then add a missing material to the list
            if(foundPiece == false)
            {
                missingPieces.Add(requiredPiece);
            }
        }
        
        //Return both the bool on if craftable and the list of pieces that are needed
        return new Tuple<bool,Array<Pieces.PieceType>>(missingPieces.Count == 0, missingPieces);
    }
}
