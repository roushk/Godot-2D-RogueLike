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
    public float cameraMovementSens = 100.0f;
    
    [Export(PropertyHint.Range,"0,10")]
    public float cameraLerpWeight = 5f;

    Vector2 cameraGoalPos = new Vector2(0,0);
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        
    }

    public override void _Input(InputEvent inputEvent)
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
        if(inputEvent.IsActionPressed("PlayerUp"))
        {
            //-y up
            this.cameraGoalPos += new Vector2(0,-1) * cameraMovementSens;
        }
        if(inputEvent.IsActionPressed("PlayerDown"))
        {
            //-y up
            this.cameraGoalPos += new Vector2(0,1) * cameraMovementSens;
        }
        if(inputEvent.IsActionPressed("PlayerLeft"))
        {
            this.cameraGoalPos += new Vector2(-1,0) * cameraMovementSens;
        }
        if(inputEvent.IsActionPressed("PlayerRight"))
        {
            this.cameraGoalPos += new Vector2(1,0) * cameraMovementSens;
        }
        this.Scale = this.Zoom/5.0f;


    }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    this.Position = this.Position.LinearInterpolate(cameraGoalPos, cameraLerpWeight * delta);
  }
}
