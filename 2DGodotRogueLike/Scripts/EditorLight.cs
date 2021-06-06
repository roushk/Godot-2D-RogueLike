using Godot;
using System;

public class EditorLight : Node2D
{
    public Sprite spriteToLight;
    public Node2D parentNode;

    [Export]
    public float rotateSpeed = 1.0f;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if(spriteToLight == null)
        {
            spriteToLight = GetParent().GetParent() as Sprite;
        }
        parentNode = GetParent() as Node2D;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        parentNode.Rotate(rotateSpeed * delta);
        Rotate(-rotateSpeed  * delta);

        if(spriteToLight != null)
        {
            Vector2 lightDir = GlobalPosition - spriteToLight.GlobalPosition;
            (spriteToLight.Material as ShaderMaterial)?.SetShaderParam("basicLightDir", new Vector3(lightDir.x,-lightDir.y,5));
        }
    }
}

