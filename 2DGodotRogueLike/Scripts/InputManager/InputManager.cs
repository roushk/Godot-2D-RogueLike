using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

//Get the input manager with 
//inputManager =  GetNode<InputManager>("/root/InputManagerSingletonNode");
public class InputManager : Node
{
    public enum KeyState
    {
        Pressed,    //pressed this frame
        Released,   //pressed last frame
        Held,       //pressed this frame and last frame
        None        //no state
    }

    Dictionary<KeyList, KeyState> keys = new Dictionary<KeyList, KeyState>();

    public override void _Ready()
    {
        //For each keys initialize to none
        foreach (KeyList item in Enum.GetValues(typeof(KeyList)))
        {
            keys[item] = KeyState.None;
        }
    }

    public KeyState GetKeyState(KeyList key)
    {
        return keys[key];
    }

    public bool IsKeyDown(KeyList key)
    {
        return keys[key] == KeyState.Pressed || keys[key] == KeyState.Held;
    }

    public bool IsKeyHeld(KeyList key)
    {
        return keys[key] == KeyState.Held;
    }

    //Handle unhandled input as per Godot suggestion for keyboard input to the game and not UI (handled input)
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey eventKey)
        {
            if(eventKey.Pressed)
            {
                if(keys[(Godot.KeyList)eventKey.Scancode] == KeyState.None)
                    keys[(Godot.KeyList)eventKey.Scancode] = KeyState.Pressed;
                    
                else if(keys[(Godot.KeyList)eventKey.Scancode] == KeyState.Pressed)
                    keys[(Godot.KeyList)eventKey.Scancode] = KeyState.Held;

            }
            else    //key not pressed this frame
            {
                if(keys[(Godot.KeyList)eventKey.Scancode] == KeyState.Pressed || keys[(Godot.KeyList)eventKey.Scancode] == KeyState.Held)
                    keys[(Godot.KeyList)eventKey.Scancode] = KeyState.Released;

                //Reset the key
                if(keys[(Godot.KeyList)eventKey.Scancode] == KeyState.Released)
                    keys[(Godot.KeyList)eventKey.Scancode] = KeyState.None;
            }
        }
    }
}
