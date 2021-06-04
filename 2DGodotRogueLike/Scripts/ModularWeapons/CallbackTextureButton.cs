using Godot;
using System;

//declaring delegate type
public delegate void BasicCallback();
public class CallbackTextureButton : TextureButton
{
    public string blueprint;

    //Callbacks for basic button functionality
    public BasicCallback onButtonPressedCallback;
    
    public BasicCallback onButtonHoveredStartCallback;
    public BasicCallback onButtonHoveredEndCallback;

    public BasicCallback onButtonPressedStartCallback;
    public BasicCallback onButtonPressedEndCallback;

    //Need to change the on button disable to maybe on pressed/hovered while disabled??    
    public BasicCallback onButtonDisabledStartCallback;
    public BasicCallback onButtonDisabledEndCallback;

    //Do the color stuff here
    [Export]
    public Color hoveredColor = new Color(0,0,1,1);
    [Export]
    public Color pressedColor = new Color(0,1,0,1);
    [Export]
    public Color disabledColor = new Color(1,0,0,1);
    [Export]
    public Color defaultColor = new Color();

    public bool changeColors = true;
    
    public override void _Ready()
    {
        defaultColor = Modulate;

        if(Disabled)
        {
            Modulate = disabledColor;
        }
    }

    public void OnButtonPressed()
    {
        if(changeColors)
            Modulate = new Color(0,0,0,0);

        if(onButtonPressedCallback != null)
            onButtonPressedCallback();
    }
    
    public void OnButtonPressedStart()
    {
        if(!Disabled)
        {
            if(changeColors)
                Modulate = pressedColor;

            if(onButtonPressedStartCallback != null)
                onButtonPressedStartCallback();
        }
    }

    public void OnButtonPressedEnd()
    {
        if(!Disabled)
        {
            if(changeColors)
                Modulate = defaultColor;

            if(onButtonPressedEndCallback != null)
                onButtonPressedEndCallback();
        }
    }
    
    public void OnButtonHoveredStart()
    {
        if(!Disabled)
        {
            if(changeColors)
                Modulate = hoveredColor;

            if(onButtonHoveredStartCallback != null)
                onButtonHoveredStartCallback();
        }
    }

    public void OnButtonHoveredEnd()
    {
        if(!Disabled)
        {
            if(changeColors)
                Modulate = defaultColor;
                
            if(onButtonHoveredEndCallback != null)
                onButtonHoveredEndCallback();
        }
    }

    public void OnButtonDisabledStart()
    {
        //Disabled = true;
        if(changeColors)
            Modulate = disabledColor;

        if(onButtonDisabledStartCallback != null)
            onButtonDisabledStartCallback();
    }

    public void OnButtonDisabledEnd()
    {
        //Disabled = false;
        if(changeColors)
            Modulate = defaultColor;
            
        if(onButtonDisabledEndCallback != null)
            onButtonDisabledEndCallback();
    }
}
