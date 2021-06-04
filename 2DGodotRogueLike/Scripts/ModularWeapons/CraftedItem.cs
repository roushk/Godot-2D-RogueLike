using Godot;
using System;
using Godot.Collections;

public class CraftedItem : Resource
{
    [Export]
    public string name { get; private set; } = "CraftedItem";
    
    [Export]
    public string iconSpriteName  { get; private set; } = "Medium_Sword";
    
    //A blueprint is made up of a list of required pieces
    public Array<Tuple<Materials.Material, Parts.PartBlueprint>> weaponParts = new Array<Tuple<Materials.Material, Parts.PartBlueprint>>{};

    //scale ratio of inv
    CallbackTextureButton CreateCraftedItemSprite(float invScale)
    {
        CallbackTextureButton baseNode = new CallbackTextureButton();
        CallbackTextureButton currentNode = baseNode;

        bool isFirst = true;
        foreach (var part in weaponParts)
        {
            //Do once
            if(isFirst)
            {
                //Set base node up
                isFirst = false;
            }
            else
            {
                //increment node to node next, doing this here cause range is 0 to n not 0 to n-1
                currentNode.AddChild(new CallbackTextureButton());
                currentNode = currentNode.GetChild(0) as CallbackTextureButton;
            }

            currentNode.TextureNormal = part.Item2.texture;
            currentNode.SetSize(new Vector2(1,1) * invScale);
            currentNode.onButtonPressedCallback = () 
            => {
                //TODO: Do stuff on button pressed for inv item, Maybe open stats of weapon or something
            };
        }
        return baseNode;
    }

    //Todo Generate sprite from the pieces, or maybe just generate a single button from them

}
