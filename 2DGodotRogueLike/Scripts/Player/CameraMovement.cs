using Godot;
using System;

public class CameraMovement : Camera2D
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
  
  InputManager inputManager;
  Vector2 cameraGoalPos = new Vector2(0,0);

  PlayerManager playerManager;
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    inputManager = GetNode<InputManager>("/root/InputManagerSingletonNode");
    playerManager = GetNode<PlayerManager>("/root/PlayerManagerSingletonNode");
    cameraGoalPos = GlobalPosition;
  }

  public override void _Input(InputEvent inputEvent)
  {
    if(zoomEnabled)
    {
      if(inputEvent.IsActionPressed("MouseWheelDown"))
      {
        //make the zoom sensitivity a scale so 1 seems reasonable as a default
        this.Zoom = this.Zoom * (1.0f + 0.1f * zoomSensitivity);
      }
      if(inputEvent.IsActionPressed("MouseWheelUp"))
      {
        this.Zoom = this.Zoom * (1.0f - 0.1f * zoomSensitivity);
      }
      this.Scale = this.Zoom / 5.0f;
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _PhysicsProcess(float delta)
  {
    if(followPlayer && playerManager.topDownPlayer != null)
    {
      cameraGoalPos = playerManager.topDownPlayer.GlobalPosition;
    }

    this.GlobalPosition = this.GlobalPosition.LinearInterpolate(cameraGoalPos, cameraLerpWeight * delta);

    if(movementEnabled)
    {
      if(inputManager.IsKeyDown(KeyList.W))
      {
        this.cameraGoalPos += new Vector2(0,-1) * cameraMovementSens * (this.Scale.x);
      }
      if(inputManager.IsKeyDown(KeyList.S))
      {
        this.cameraGoalPos += new Vector2(0,1) * cameraMovementSens* (this.Scale.x);
      }
      if(inputManager.IsKeyDown(KeyList.D))
      {
        this.cameraGoalPos += new Vector2(1,0) * cameraMovementSens* (this.Scale.x);
      }
      if(inputManager.IsKeyDown(KeyList.A))
      {
        this.cameraGoalPos += new Vector2(-1,0) * cameraMovementSens* (this.Scale.x);
      }
    } 
  }
}
