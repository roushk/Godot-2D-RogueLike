using Godot;
using System;

public class CameraMovement : Camera2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    public float zoomSensitivity = 1.0f;

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
    }

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
