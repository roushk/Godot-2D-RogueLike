using Godot;
using System;
using Godot.Collections;

public class Inventory : Node
{
  public Dictionary<Materials.Material, int> stackableItems = new Dictionary<Materials.Material, int>();
  public System.Collections.Generic.List<Tuple<string, BaseBlueprint>> uniqueItems = new System.Collections.Generic.List<Tuple<string, BaseBlueprint>>();
}
