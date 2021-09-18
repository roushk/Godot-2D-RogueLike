using Godot;
using System;
using Godot.Collections;

public class Inventory : Control
{
  //How to deal with stack sizes?
  Dictionary<Materials.Material, int> stackableItems = new Dictionary<Materials.Material, int>();
  System.Collections.Generic.List<Tuple<string, BaseBlueprint>> uniqueItems = new System.Collections.Generic.List<Tuple<string, BaseBlueprint>>();
  
  Dictionary<Materials.Material, HBoxContainer> stackableItemsUI = new Dictionary<Materials.Material, HBoxContainer>();

  PackedScene CallbackTextureButtonWithTextScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/CallbackTextureButtonWithTextScene.tscn");

  //Load sprite frames to populate inventory with
  SpriteFrames materialSpriteFrames = ResourceLoader.Load<SpriteFrames>("res://Assets/Art/SpriteSheets/OreChunkSpriteFrames.tres");

  protected PlayerManager playerManager;
  
  GridContainer stackablesGridContainer;
  GridContainer uniqueGridContainer;

  bool ranFirstTimeInit = false;

  public void AddMaterial(Materials.Material material, int count)
  {
    int currVal = 0;
    if(stackableItems.TryGetValue(material, out currVal))
    {
      stackableItemsUI[material].Visible = true;
      stackableItems[material] = currVal + count;
      stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: " + stackableItems[material].ToString();

      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].Visible = true;
      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: " + stackableItems[material].ToString();
    }
    else  //if material doesn't exist then make a new one and add it
    {
      stackableItemsUI[material].Visible = true;
      //Make UI visible
      stackableItems.Add(material, count);
      stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: " + stackableItems[material].ToString();

      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].Visible = true;
      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: " + stackableItems[material].ToString();
      
    }
    Console.WriteLine("Added " + count.ToString() + " of Material " + material.ToString() + " to Inventory");
  }

  public void AddUniqueItem(string inventoryObjectName,	BaseBlueprint blueprint)
  {
    //TODO later selecting weapon brings up weapon menu to change stance/combo/special etc 
    uniqueItems.Add(new Tuple<string,	BaseBlueprint>(inventoryObjectName,blueprint));
    Console.WriteLine("Added Unique Item " + inventoryObjectName + " to Inventory");
  }

  //Returns whether the inventory has the material
  public bool HasMaterial(Materials.Material material, int count)
  {
    //deal with asking for 0 for some reason
    if(count == 0)
      return true;
  
    int currVal = 0;
    if(stackableItems.TryGetValue(material, out currVal))
    {
      return stackableItems[material] >= count;
    }
    //if we don't have the material then we can't have the material
    return false;
  }

  //Removes the material from the inventory and assumes the inventory has enough
  public void RemoveMaterial(Materials.Material material, int count)
  {
    //deal with removing 0 for some reason
    if(count == 0)
      return;

    int currVal = 0;
    if(stackableItems.TryGetValue(material, out currVal))
    {
      stackableItems[material] -= count;

      if(stackableItems[material] == 0)
      {
        //TODO convert this to a function call
        stackableItemsUI[material].GetChild<RichTextLabel>(1).BbcodeText = material.ToString() + "\n" + "Amount: 0";
        stackableItemsUI[material].Visible = false;
        playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].Visible = true;
        playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: 0";

        //Make UI not visible
      }
      stackableItemsUI[material].GetChild<RichTextLabel>(1).BbcodeText = material.ToString() + "\n" + "Amount: " + stackableItems[material].ToString();
      playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI[material].GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData").BbcodeText = material.ToString() + "\n" + "Amount: " + stackableItems[material].ToString();
    }
  }

  public HBoxContainer CreateStackableItemInventoryUI(Materials.Material material)
  {
    HBoxContainer hBox = CallbackTextureButtonWithTextScene.Instance<HBoxContainer>();

    CallbackTextureButton texButton = hBox.GetNode<CallbackTextureButton>("VBoxContainer/HSplitContainer/PartIcon");
    RichTextLabel text = hBox.GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData");

    texButton.TextureNormal = materialSpriteFrames.GetFrame("Ore Chunks", (int)material);
    texButton.Modulate = Colors.White;
    texButton.SelfModulate = Colors.White;
    texButton.changeColors = false;

    text.BbcodeText = material.ToString() + "\n" + "Amount: 0";
    text.BbcodeEnabled = true;
    text.RectMinSize = new Vector2(text.RectMinSize.x,CraftingMaterialSystem.GetMinYSizeFromRichTextLabel(text));
    hBox.Visible = false;

    return hBox;
  }

  public override void _Ready()
  {
    //Set up containers
    stackablesGridContainer = GetNode<GridContainer>("PartSelectionDetail/Inventory/InventoryTabs/Ores/GridContainer");
    uniqueGridContainer = GetNode<GridContainer>("PartSelectionDetail/Inventory/InventoryTabs/Weapons/GridContainer");
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
        
        //Init to zero
        stackableItems.Add(material, 0);

        HBoxContainer hBox = CreateStackableItemInventoryUI(material);
        stackableItemsUI.Add(material, hBox);
        stackablesGridContainer.AddChild(hBox);

        HBoxContainer hBox2 = CreateStackableItemInventoryUI(material);

        playerManager.topDownPlayer.playerCraftingUI.stackableItemsUI.Add(material, hBox2);
        playerManager.topDownPlayer.playerCraftingUI.inventoryOres.AddChild(hBox2);
      }
    }
  }
}
