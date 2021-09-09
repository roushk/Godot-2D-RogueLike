using Godot;
using System;

public class OptionsMenuPopupFix : OptionButton
{
  [Export]
  public Vector2 offset;
  public override void _Ready()
  {
      //-y up
    offset = new Vector2(0, GetPopup().RectSize.y);
  }
  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta)
  {
    GetPopup().SetPosition(RectGlobalPosition + offset);
  }
}
