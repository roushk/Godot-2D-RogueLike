using Godot;
using System;

public class PlayerUI : Control
{
	PlayerManager playerManager;
	private PackedScene townScene = ResourceLoader.Load<PackedScene>("res://Scenes/Levels/Town.tscn");

	public Button returnToTownButton;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
		returnToTownButton = GetNode<Button>("ReturnToTownButton");
		returnToTownButton.Visible = false;
	}

	public void _on_ReturnToTownButton_pressed()
	{
		playerManager.ChangeLevelTo(townScene, "/root/TownRootNode/PlayerSpawnLocation_DungeonExit");
	}


}
