using Godot;
using System;
using System.Collections.Generic;

public class InventoryUI : Control
{  
  Dictionary<Materials.Material, HBoxContainer> stackableItemsUI = new Dictionary<Materials.Material, HBoxContainer>();

  PackedScene CallbackTextureButtonWithTextScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/CallbackTextureButtonWithTextScene.tscn");

  //Load sprite frames to populate inventory with
  SpriteFrames materialSpriteFrames = ResourceLoader.Load<SpriteFrames>("res://Assets/Art/SpriteSheets/OreChunkSpriteFrames.tres");

  protected PlayerManager playerManager;
  
  GridContainer stackablesGridContainer;
  GridContainer uniqueGridContainer;

  bool ranFirstTimeInit = false;

  //Update the visuals for the material to the count passed
  public void UpdateMaterialVisual(Materials.Material material, int count)
  {
    //Visible
    if(count > 0)
    {
      stackableItemsUI[material].Visible = true;
      stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: " + count.ToString();
      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].Visible = true;
      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: " + count.ToString();
    }
    else
    {
      //Make UI not visible
      stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: 0";
      stackableItemsUI[material].Visible = false;
      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].Visible = false;
      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: 0";

    }
  }

  public void AddUniqueItemVisual(string inventoryObjectName,	Parts.ConstructedWeapon weapon)
  {
    //TODO later selecting weapon brings up weapon menu to change stance/combo/special etc 
    Console.WriteLine("Added Unique Item " + inventoryObjectName + " to Inventory");

    HBoxContainer hBox = CallbackTextureButtonWithTextScene.Instance<HBoxContainer>();

    CallbackTextureButton texButton = hBox.GetNode<CallbackTextureButton>("VBoxContainer/HSplitContainer/PartIcon");
    RichTextLabel text = hBox.GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData");
    
    texButton.TextureNormal = weapon.texture;
    texButton.Modulate = Colors.White;
    texButton.SelfModulate = Colors.White;
    texButton.changeColors = true;
    texButton.Disabled = false;

    //Set material selection callback
    texButton.onButtonPressedCallback = () => {playerManager.topDownPlayer.SetCurrentWeapon(weapon);};

    text.BbcodeText = inventoryObjectName + "\n" + weapon.detailText;
    text.BbcodeEnabled = true;
    text.RectMinSize = new Vector2(text.RectMinSize.x,CraftingMaterialSystem.GetMinYSizeFromRichTextLabel(text));
    text.RectSize = text.RectMinSize;

    uniqueGridContainer.AddChild(hBox);
  }


  public HBoxContainer CreateStackableItemInventoryUI(Materials.Material material, BasicCallback callback = null, bool changeColors = false)
  {
    HBoxContainer hBox = CallbackTextureButtonWithTextScene.Instance<HBoxContainer>();

    CallbackTextureButton texButton = hBox.GetNode<CallbackTextureButton>("VBoxContainer/HSplitContainer/PartIcon");
    RichTextLabel text = hBox.GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData");

    texButton.TextureNormal = materialSpriteFrames.GetFrame("Ore Chunks", (int)material);
    texButton.Modulate = Colors.White;
    texButton.SelfModulate = Colors.White;
    texButton.changeColors = changeColors;
    texButton.Disabled = false;
    //Set material selection callback
    texButton.onButtonPressedCallback = callback;

    text.BbcodeText = material.ToString() + "\n" + "Amount: 0";
    text.BbcodeEnabled = true;
    text.RectMinSize = new Vector2(text.RectMinSize.x,CraftingMaterialSystem.GetMinYSizeFromRichTextLabel(text));
    text.RectSize = text.RectMinSize;
    hBox.Visible = false;

    return hBox;
  }

  public override void _Ready()
  {
    //Set up containers
    stackablesGridContainer = GetNode<GridContainer>("InventoryRect/Inventory/InventoryTabs/Ores/GridContainer");
    uniqueGridContainer = GetNode<GridContainer>("InventoryRect/Inventory/InventoryTabs/Weapons/GridContainer");

    //Setup player manager
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
  }

  public override void _Process(float delta)
  {
    if(ranFirstTimeInit == false)
    {
      ranFirstTimeInit = true;

      //iterate every material
      foreach(Materials.Material material in Enum.GetValues(typeof(Materials.Material)))
      {
        //For now just break when at bronze, only generate Iron -> Cobalt
        if(material == Materials.Material.Bronze)
        {
          break;
        }
        
        HBoxContainer hBox = CreateStackableItemInventoryUI(material);
        stackableItemsUI.Add(material, hBox);
        stackablesGridContainer.AddChild(hBox);

        //Change colors and set selected material for the inventory items in the crafting menu
        HBoxContainer hBox2 = CreateStackableItemInventoryUI(material, () => 
        {
          //Update the cost of the weapon
          playerManager.topDownPlayer.playerCraftingUI.selectedInventoryMaterial = material;
          if(playerManager.topDownPlayer.playerCraftingUI.selectedWeaponBPNode != null)
            playerManager.topDownPlayer.playerCraftingUI.selectedWeaponBPNode.part.currentMaterial = material;

          //Set selected material of part
          if(playerManager.topDownPlayer.playerCraftingUI.selectedPart != null)
            playerManager.topDownPlayer.playerCraftingUI.selectedPart.defaultColor = Materials.MaterialTints.tints[material];

          //Set currently selected part to null
          playerManager.topDownPlayer.playerCraftingUI.UpdateCurrentlySelectedPart(null);

          //updates interally if we are done selecting materials
          playerManager.topDownPlayer.playerCraftingUI.GetWeaponMaterialCost(playerManager.playerInventory);
          playerManager.topDownPlayer.playerCraftingUI.GeneratePartVisualizerUIFromCurrentParts();

        }, true);

        playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI.Add(material, hBox2);
        playerManager.topDownPlayer.playerCraftingUI.inventoryOres.AddChild(hBox2);

        //update the material in case we already have some
        playerManager.topDownPlayer.playerInventoryUI.UpdateMaterialVisual(material, playerManager.playerInventory.GetMaterialCount(material));
      }
    }
  }
}
