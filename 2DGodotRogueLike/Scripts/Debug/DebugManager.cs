using Godot;
using System;
using System.Collections.Generic;

public enum MouseOptions
{
  AStar_Pathfind,
  AStar_NodeInfo,
  DirectedGraph_ToParent,
  Character_Select,
};


public class DebugManager : Node2D
{
#region Variables

  public MouseOptions currentMouseOption = MouseOptions.AStar_Pathfind;  
  public OptionButton ActiveOverlayOptions;
  public OptionButton MouseOptionsButton;

  public Camera2D debugCamera;
  public Camera2D playerCamera;

  public bool setAStarStart = false;
  public bool setAStarDest = false;
  public AStar.PathState AStarState = AStar.PathState.None;
  public Vector2 AStarStart;
  public Vector2 AStarDest;

  public Label currentMouseSelectionAStar_NodeCoords;
  public Label currentMouseSelectionAStar_GivenCost;
  public Label currentMouseSelectionAStar_Heuristic;
  public Label currentMouseSelectionAStar_ParentNodeCoords;
  public Label currentMouseSelectionAStar_NodeState;

  public AStar.AStarNode currentMouseSelectionNode = new AStar.AStarNode();
  public MarginContainer DebugUI;

  public TestLevelGeneration testLevelGeneration;

  public bool playerMode = false;

  InputManager inputManager;

  Physics2DDirectSpaceState spaceState;

#endregion

  public override void _Ready()
  {
    inputManager = GetNode<InputManager>("/root/InputManagerSingletonNode");
  }

  public void PostLevelGenInit()
  {
    testLevelGeneration = this.GetParent().GetNode("TestLevelGenNode") as TestLevelGeneration;
    DebugUI = testLevelGeneration.GetNode("Camera2D/GUI") as MarginContainer;
    debugCamera = testLevelGeneration.GetNode("Camera2D") as Camera2D;

    currentMouseSelectionAStar_NodeCoords = testLevelGeneration.GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General7") as Label;
    currentMouseSelectionAStar_GivenCost = testLevelGeneration.GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General8") as Label;
    currentMouseSelectionAStar_Heuristic = testLevelGeneration.GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General9") as Label;
    currentMouseSelectionAStar_ParentNodeCoords = testLevelGeneration.GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General10") as Label;
    currentMouseSelectionAStar_NodeState = testLevelGeneration.GetNode("Camera2D/MouseInfoUI/VBoxContainer/HSplitContainer/VBoxContainer2/General11") as Label;


    ActiveOverlayOptions = testLevelGeneration.GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer5/SelectedOverlay") as OptionButton;

    MouseOptionsButton = testLevelGeneration.GetNode("Camera2D/GUI/VBoxContainer/HSplitContainer6/MouseOptions") as OptionButton;

    //Generate the options menu from the dict keys to make sure they are good with 0 still being no overlays
    foreach (var item in Enum.GetNames(typeof(MouseOptions)))
    {
      MouseOptionsButton.AddItem(item);
    }
    //Generate the options menu from the dict keys to make sure they are good with 0 still being no overlays
    foreach (var item in testLevelGeneration.VisualizationMaps)
    {
      ActiveOverlayOptions.AddItem(item.Key);
    }
  }

  public void UpdateMouseInfoUI()
  {
    //https://docs.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings
    //5 points after decimal
    currentMouseSelectionAStar_NodeCoords.Text = currentMouseSelectionNode.pos.ToString("F5");
    currentMouseSelectionAStar_GivenCost.Text = currentMouseSelectionNode.givenCost.ToString("F5");
    currentMouseSelectionAStar_Heuristic.Text = currentMouseSelectionNode.heuristic.ToString("F5");
    if(currentMouseSelectionNode.parent != null)  
      currentMouseSelectionAStar_ParentNodeCoords.Text = currentMouseSelectionNode.parent.pos.ToString("F5");
    currentMouseSelectionAStar_NodeState.Text = currentMouseSelectionNode.state.ToString();
  }

  public void SetPlayerMode(bool _playerMode)
  {
    playerMode = _playerMode;
    if(playerMode)
    {
      debugCamera.Current = false;
      (debugCamera as CameraMovement).movementEnabled = false;
      DebugUI.Visible = false;
      if(playerCamera != null)
        playerCamera.Current = true;
    }
    else
    {
      (debugCamera as CameraMovement).movementEnabled = true;
      debugCamera.Current = true;
      DebugUI.Visible = true;
      if(playerCamera != null)
        playerCamera.Current = false;
    }
  }

  //Select with mouse position
  public override void _UnhandledInput(InputEvent inputEvent)
  {

    if(currentMouseOption == MouseOptions.DirectedGraph_ToParent)
    {
      if (@inputEvent is InputEventMouseButton mouseClick && (mouseClick.Pressed && mouseClick.ButtonIndex == (int)Godot.ButtonList.Left))
      {
        //World space -> Map space where coordinates are
        Vector2 clickedPos = testLevelGeneration.ForegroundMap.WorldToMap(testLevelGeneration.ForegroundMap.GetLocalMousePosition());
        ChokePointFinder.CPFNode foundNode;
        if(testLevelGeneration.FindNodeAtPos(clickedPos, testLevelGeneration.cpfRootNode, out foundNode))
        {
          //Reset the visuals
          testLevelGeneration.CPF.UpdateDirectedMapVis(testLevelGeneration.cpfRootNode);
          testLevelGeneration.DrawParentTree(foundNode);
          //Console.WriteLine("Found Node! at " + clickedPos.ToString() + " Where node exists at " + foundNode.pos.ToString());
        }
      }
    }
    else if (currentMouseOption == MouseOptions.AStar_Pathfind)
    {
      if(@inputEvent is InputEventMouseButton mouseClick && mouseClick.Pressed)
      {
        if (mouseClick.ButtonIndex == (int)Godot.ButtonList.Right)
        {
          AStarState = AStar.PathState.None;
          Vector2 newAStarDest = testLevelGeneration.ForegroundMap.WorldToMap(testLevelGeneration.ForegroundMap.GetLocalMousePosition());

          if(newAStarDest.x >= testLevelGeneration.width || newAStarDest.x <= 0)
            return;
          if(newAStarDest.y >= testLevelGeneration.height || newAStarDest.y <= 0)
            return;
            
          //Don't allow walls to be set as dest or start
          if(testLevelGeneration.terrainMap[(int)newAStarDest.x,(int)newAStarDest.y] == 0)
          {
            //Remove old one
            testLevelGeneration.VisualizationMaps["AStar Overlay"].SetCell((int)AStarDest.x,(int)AStarDest.y, -1);

            AStarDest = newAStarDest;
            //Set dest
            setAStarDest = true;
            testLevelGeneration.VisualizationMaps["AStar Overlay"].SetCell((int)AStarDest.x,(int)AStarDest.y, 11);
          }
          else
          {
            AStarDest = Vector2.Zero;
            setAStarDest = false;
          }
        }

        if (mouseClick.ButtonIndex == (int)Godot.ButtonList.Left)
        {
          AStarState = AStar.PathState.None;
          Vector2 newAStarStart = testLevelGeneration.ForegroundMap.WorldToMap(testLevelGeneration.ForegroundMap.GetLocalMousePosition());

          if(newAStarStart.x >= testLevelGeneration.width || newAStarStart.x <= 0)
            return;
          if(newAStarStart.y >= testLevelGeneration.height || newAStarStart.y <= 0)
            return;
            
          //Dont allow walls to be set as dest or start
          if(testLevelGeneration.terrainMap[(int)newAStarStart.x,(int)newAStarStart.y] == 0)
          {
            testLevelGeneration.VisualizationMaps["AStar Overlay"].SetCell((int)AStarStart.x,(int)AStarStart.y, -1);

            AStarStart = newAStarStart;
            //Set start
            setAStarStart = true;
            testLevelGeneration.VisualizationMaps["AStar Overlay"].SetCell((int)AStarStart.x,(int)AStarStart.y, 4);
          }
          else
          {
            AStarStart = Vector2.Zero;
            setAStarStart = false;
          }
        }
      }
      if(setAStarStart && setAStarDest && AStarState == AStar.PathState.None)
      {
        testLevelGeneration.AStarMap = new AStar.AStarMap(testLevelGeneration.terrainMap, testLevelGeneration.width, testLevelGeneration.height);
        //World space -> Map space where coordinates are
        testLevelGeneration.AStarPather.InitPather(AStarStart, AStarDest, testLevelGeneration.AStarMap);
        //if not realtime then just generate the path
        
        if(!testLevelGeneration.realtimeAStar)
        {
          AStarState = testLevelGeneration.AStarPather.GeneratePath(testLevelGeneration.realtimeAStar);
          testLevelGeneration.AStarPather.UpdateMapVisual();
        }
        else
        {
          AStarState = AStar.PathState.Searching;
        }
      }
    }
    else if (currentMouseOption == MouseOptions.AStar_NodeInfo)
    {
      if(@inputEvent is InputEventMouseButton mouseClick && mouseClick.Pressed && mouseClick.ButtonIndex == (int)Godot.ButtonList.Left)
      {
        if(testLevelGeneration.AStarPather.map != null)
        {
          currentMouseSelectionNode = testLevelGeneration.AStarPather.map.GetNodeAt(testLevelGeneration.ForegroundMap.WorldToMap(testLevelGeneration.ForegroundMap.GetLocalMousePosition()));
        }

        UpdateMouseInfoUI();
      }
    }
    else if(currentMouseOption == MouseOptions.Character_Select)
    {
      if(@inputEvent is InputEventMouseButton mouseClick && mouseClick.Pressed && mouseClick.ButtonIndex == (int)Godot.ButtonList.Left)
      {
        //Returns some JSON type stuff????
        var result = spaceState.IntersectPoint(GetGlobalMousePosition(),32, new Godot.Collections.Array { this });

        
        foreach (Godot.Collections.Dictionary item in result)
        {
          Console.WriteLine("Point intersected with: " + item.ToString() + " Is of type " + item["collider"].GetType().ToString());
        }
      }
    }

    if(inputManager.IsKeyPressed(KeyList.M))
    {
      SetPlayerMode(!playerMode);
    }
  }
  
  public override void _PhysicsProcess(float delta)
  {
    spaceState = GetWorld2d().DirectSpaceState;

  }
}
