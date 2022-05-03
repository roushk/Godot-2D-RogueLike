using Godot;
using System;

public class CameraMovement : Camera
{
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  [Export]
  public float zoomSensitivity = 1.0f;
  [Export]
  public float cameraMovementSens = 5.0f;
  
  [Export(PropertyHint.Range,"0,10")]
  public float cameraLerpWeight = 5f;

  [Export]
  public bool movementEnabled = true;

  [Export]
  public bool zoomEnabled = true;

  [Export]
  public bool followPlayer = false;
  
	[Export]
	Vector3 cameraGoalOffsetPos = new Vector3(0,10,0);

  InputManager inputManager;
  Vector3 cameraGoalPos = new Vector3(0,0,0);


  PlayerManager playerManager;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
	inputManager = GetNode<InputManager>("/root/InputManagerSingletonNode");
	playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
	cameraGoalPos = Translation;
  }

  public override void _Input(InputEvent inputEvent)
  {
	//TODO Maybe redo zoom?
	//if(zoomEnabled)
	//{
	//  if(inputEvent.IsActionPressed("MouseWheelDown"))
	//  {
	//    //make the zoom sensitivity a scale so 1 seems reasonable as a default
	//    this.Zoom = this.Zoom * (1.0f + 0.1f * zoomSensitivity);
	//  }
	//  if(inputEvent.IsActionPressed("MouseWheelUp"))
	//  {
	//    this.Zoom = this.Zoom * (1.0f - 0.1f * zoomSensitivity);
	//  }
	//  this.Scale = this.Zoom / 5.0f;
	//}
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
		if(followPlayer && playerManager.topDownPlayer != null)
		{
			cameraGoalPos = playerManager.topDownPlayer.Translation;
		}

		this.Translation = this.Translation.LinearInterpolate(cameraGoalPos + cameraGoalOffsetPos, cameraLerpWeight * delta);

		if(movementEnabled)
		{
			//TODO redo so camera is on specific plane height
			if(inputManager.IsKeyDown(KeyList.W))
			{
			this.cameraGoalPos += new Vector3(0,-1,0) * cameraMovementSens * (this.Scale.x);
			}
			if(inputManager.IsKeyDown(KeyList.S))
			{
			this.cameraGoalPos += new Vector3(0,1,0) * cameraMovementSens* (this.Scale.x);
			}
			if(inputManager.IsKeyDown(KeyList.D))
			{
			this.cameraGoalPos += new Vector3(1,0,0) * cameraMovementSens* (this.Scale.x);
			}
			if(inputManager.IsKeyDown(KeyList.A))
			{
			this.cameraGoalPos += new Vector3(-1,0,0) * cameraMovementSens* (this.Scale.x);
			}
		} 
  }
}
