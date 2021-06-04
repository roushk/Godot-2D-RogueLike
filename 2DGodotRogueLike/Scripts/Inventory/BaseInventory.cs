using Godot;
using System;
using Godot.Collections;

public class BaseInventory : Node
{
    Dictionary<Materials.Material, int> stackableItems = new Dictionary<Materials.Material, int>();
    System.Collections.Generic.List<Tuple<string, BaseBlueprint>> uniqueItems = new System.Collections.Generic.List<Tuple<string, BaseBlueprint>>();
}
