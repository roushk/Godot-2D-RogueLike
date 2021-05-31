using Godot;
using System;

public class BPTextureButton : Node
{
    //TexButton
    //OnHoverFunc
    //OnHoverModulation
    //OnPressedModulation
    //OnPressedFunc

    //declaring delegate type
    public delegate void OnButtonPressedCallback();
    OnButtonPressedCallback callback;

    void OnButtonPressed()
    {
        callback();
    }
}
