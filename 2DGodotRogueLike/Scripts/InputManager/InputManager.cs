using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

//Get the input manager with 
//inputManager =  GetNode<InputManager>("/root/InputManagerSingletonNode");
public class InputManager : Node
{
  public enum KeyState
  {
    Pressed,  //pressed this frame
    Released, //pressed last frame
    Held,     //pressed this frame and last frame
    None      //no state
  }

  Dictionary<KeyList, KeyState> keys = new Dictionary<KeyList, KeyState>();
  Dictionary<KeyList, KeyState> keysThisFrame = new Dictionary<KeyList, KeyState>();

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

  //pressed this frame
  public bool IsKeyPressed(KeyList key)
  {
    return keys[key] == KeyState.Pressed;
  }

  //pressed this frame
  //ONLY WORKS IN _PhysicsProcess as _Process can run multiple times before _PhysicsProcess is updated
  public bool IsKeyReleased(KeyList key)
  {
    return keys[key] == KeyState.Released;
  }
  
  //Down = pressed this frame or held down
  public bool IsKeyDown(KeyList key)
  {
    return keys[key] == KeyState.Pressed || keys[key] == KeyState.Held;
  }
  
  //If the key is held down
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
          keysThisFrame[(Godot.KeyList)eventKey.Scancode] = KeyState.Pressed;
      }
      else if(eventKey.Echo)
      {
        if(keys[(Godot.KeyList)eventKey.Scancode] == KeyState.Pressed)
          keysThisFrame[(Godot.KeyList)eventKey.Scancode] = KeyState.Held;
      }
    }
  }

  public override void _PhysicsProcess(float delta)
  {

    //LINQ magic allowing me to edit the dictionary as we are iterating entire key list
    foreach(var pair in keys.ToList())
    {
      if(pair.Value == KeyState.Pressed || pair.Value == KeyState.Held)
        keys[pair.Key] = KeyState.Released;

      //Reset the key
      else if(pair.Value == KeyState.Released)
        keys[pair.Key] = KeyState.None;
    }

    //Update Keys at set time
    foreach(var pair in keysThisFrame)
    {
      if(pair.Value == KeyState.Pressed || pair.Value == KeyState.Held)
        keys[pair.Key] = pair.Value;
    }

    keysThisFrame.Clear();
  }
}
