using Godot;
using System;

public class CraftingSystemCheatMenuUI : Control
{
  PlayerManager playerManager;
  InputManager inputManager;
  OptionButton oreSpawnerSelectionOptionsButton;

  public void GivePlayerOre()
  {
    playerManager.playerInventory.AddMaterial((Materials.Material)oreSpawnerSelectionOptionsButton.Selected, 10);
  }
  
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    inputManager = GetNode<InputManager>("/root/InputManagerSingletonNode");

    oreSpawnerSelectionOptionsButton = GetNode("VBoxContainer/HSplitContainer/OptionButton") as OptionButton;

    //Generate the options menu from the dict keys to make sure they are good with 0 still being no overlays
    foreach (Materials.Material item in Enum.GetValues(typeof(Materials.Material)))
    {
      if(item == Materials.Material.Bronze)
        break;

      oreSpawnerSelectionOptionsButton.AddItem(item.ToString());
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    if(inputManager.IsKeyPressed(KeyList.N))
    {
      Visible = !Visible;
    }
  }
}
