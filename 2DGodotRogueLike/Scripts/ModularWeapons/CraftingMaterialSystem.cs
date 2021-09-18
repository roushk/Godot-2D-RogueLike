using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CraftingMaterialSystem : Control
{
  //Signals

  public void SwitchToIronOnButtonPressed()
  {
    ingot.Modulate = Materials.MaterialTints.tints[Materials.Material.Iron];
  }

  public void SwitchToCopperOnButtonPressed()
  {
    ingot.Modulate = Materials.MaterialTints.tints[Materials.Material.Copper];
  }
  
  public void SwitchToTinOnButtonPressed()
  {
    ingot.Modulate = Materials.MaterialTints.tints[Materials.Material.Tin];
  }

  public void SwitchToAdamantiteOnButtonPressed()
  {
    ingot.Modulate = Materials.MaterialTints.tints[Materials.Material.Adamantite];
  }


  public void _on_SelectMaterialsButton_toggled(bool toggled)
  {
    if(toggled)
    {
      SetModeMaterialSelection();
    }
    else
    {
      SetModePartSelection();
    }
  }

#region Variables
  //Dict of material tints to lookup of pieces

  //Dict of type to list of pieces
  Dictionary<Parts.PartType, Godot.Collections.Array<Parts.PartBlueprint>> allPartsDict = new Dictionary<Parts.PartType,Godot.Collections. Array<Parts.PartBlueprint>>();

  System.Collections.Generic.List<Parts.PartBlueprint> currentParts = new System.Collections.Generic.List<Parts.PartBlueprint>();

  Parts.WeaponBlueprintNode weaponRootNode = new Parts.WeaponBlueprintNode();

  System.Collections.Generic.List<Parts.AttachPoint> attachPoints = new System.Collections.Generic.List<Parts.AttachPoint>();

  CallbackTextureButton ingot;

  BaseBlueprint selectedBlueprint;
  Parts.AttachPoint selectedAttachPoint;
  
  public CallbackTextureButton selectedPart;
  public Parts.WeaponBlueprintNode selectedWeaponBPNode;
  public Materials.Material selectedInventoryMaterial = Materials.Material.Undefined;

  Dictionary<string,BaseBlueprint> blueprints = new Dictionary<string,BaseBlueprint>();

  //Load packed scenes 
  PackedScene CallbackTextureButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/CallbackTextureButtonScene.tscn");
  PackedScene CallbackTextureButtonWithTextScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/CallbackTextureButtonWithTextScene.tscn");

  RichTextLabel currentBlueprintText;

  public Dictionary<Materials.Material, HBoxContainer> stackableItemsUI = new Dictionary<Materials.Material, HBoxContainer>();
  public GridContainer inventoryOres;

  string fontBBcodePrefix = "[center][b]";

  Texture attachPointTex;
  BitMap attachPointBitmask;
  const string attachPointAssetStr = "res://Assets/Art/My_Art/AttachPoint.png";

  Node partVisualizerContainer;
  Node currentBlueprintDetailContainer;
  Node blueprintContainer;
  Node newPartSelectionContainer;

  Vector2 partVisualizerScale = new Vector2(4,4);

  //Minimum box size for each sprite, also means the max sprite size is 30x30 with 1 pixel border
  Vector2 MinPartSelectionSize = new Vector2(32,32);

  float basePartVisualizerScale = 8.0f;

  public enum CraftingSystemMode
  {
    PartSelection,
    MaterialSelection
  }

  public CraftingSystemMode currentMode = CraftingSystemMode.PartSelection;

  bool playerCanCraftWeapon = false;

  //Todo replace this with something significantly better..
  //It will help to change the type from TextureButton to derived class like the other BP thing with callbacks
  int partNum = 0;

  //Paths
  const string FullBPDir = "res://Data/Blueprints/";
  const string FullSpriteDir = "res://Assets/Art/My_Art/BlueprintIcons/";

  #endregion

  void SetModeMaterialSelection()
  {
    currentMode = CraftingSystemMode.MaterialSelection;
    ClearPartSelection();
    GeneratePartVisualizerUIFromCurrentParts();

    GetNode<RichTextLabel>("PartSelection/BlueprintStuff/PartInformationTitle").Visible = false;
    GetNode<ScrollContainer>("PartSelection/BlueprintStuff/PartSelectionScrollContainer").Visible = false;

    GetNode<RichTextLabel>("PartSelection/BlueprintStuff/OreInInventoryTitle").Visible = true;
    GetNode<ScrollContainer>("PartSelection/BlueprintStuff/OreInventorySelection").Visible = true;
  }

  void SetModePartSelection()
  {
    currentMode = CraftingSystemMode.PartSelection;
    ClearPartSelection();
    GeneratePartVisualizerUIFromCurrentParts();

    GetNode<RichTextLabel>("PartSelection/BlueprintStuff/PartInformationTitle").Visible = true;
    GetNode<ScrollContainer>("PartSelection/BlueprintStuff/PartSelectionScrollContainer").Visible = true;

    GetNode<RichTextLabel>("PartSelection/BlueprintStuff/OreInInventoryTitle").Visible = false;
    GetNode<ScrollContainer>("PartSelection/BlueprintStuff/OreInventorySelection").Visible = false;
  }

  //Load parts into parts dictionary
  void LoadAllParts()
  {
    //Generate texture and bitmask for the attachment nodes
    attachPointTex = (Texture)GD.Load(attachPointAssetStr);
    attachPointBitmask = new BitMap();
    attachPointBitmask.CreateFromImageAlpha(attachPointTex.GetData());
    
    //https://www.c-sharpcorner.com/article/loop-through-enum-values-in-c-sharp/
    //For each Piece type generate an array in the pieces dict
    foreach (Parts.PartType type in Enum.GetValues(typeof(Parts.PartType)))
    { 
      if(type == Parts.PartType.Undefined)
        continue;
      allPartsDict[type] = new Godot.Collections.Array<Parts.PartBlueprint>();
    }

    Godot.Collections.Array<Parts.PartBlueprint> createdParts = new Godot.Collections.Array<Parts.PartBlueprint>();

    //Read json file into text
    Godot.File file = new Godot.File();
    file.Open("res://Data/PartsList.json", Godot.File.ModeFlags.Read);
    string jsonText = file.GetAsText();
    file.Close();

    //Construct dict of stuff
    Godot.Collections.Array ParsedData = Godot.JSON.Parse(jsonText).Result as Godot.Collections.Array;

    //Parse data based on Resource
    foreach (Godot.Collections.Dictionary data in ParsedData)
    {
      Parts.PartBlueprint partBP = new Parts.PartBlueprint();
      partBP.name = data["partName"] as string;
      partBP.texture = (Texture)GD.Load(data["partTextureDir"] as string);
      
      if(partBP.texture == null)
      {
        throw new Exception("Missing Texture :\"" + data["partTextureDir"] as string +"\"");
      }

      partBP.partType = Parts.PartTypeConversion.FromString(data["partType"] as string);
      
      //Don't ask
      partBP.materialCost = (int)(float)data["partCost"];
      //Ok, it's because json uses floats only so object -> float -> int


      Godot.Collections.Dictionary basicAttachPt = data["baseAttachPoint"] as Godot.Collections.Dictionary;
      partBP.baseAttachPoint = new Vector2((int)(float)basicAttachPt["x"],(int)(float)basicAttachPt["y"]);

      Godot.Collections.Dictionary partAttributes = data["partAttributes"] as Godot.Collections.Dictionary;
      partBP.stats.baseSlashDamage =  (int)(float)partAttributes["baseSlashDamage"];
      partBP.stats.baseStabDamage =   (int)(float)partAttributes["baseStabDamage"];
      partBP.stats.baseAttackSpeed =  (int)(float)partAttributes["baseAttackSpeed"];
      partBP.stats.baseSwingSpeed =   (int)(float)partAttributes["baseSwingSpeed"];
      partBP.stats.baseLength =       (int)(float)partAttributes["baseLength"];
      partBP.stats.specialStat =      partAttributes["specialStat"] as string;

      foreach (Godot.Collections.Dictionary partAttachPoints in data["partAttachPoints"] as Godot.Collections.Array)
      {
        Godot.Collections.Array<Parts.PartType> types = new Godot.Collections.Array<Parts.PartType>();
        int x = (int)(float)partAttachPoints["x"];
        int y = (int)(float)partAttachPoints["y"];
        foreach (var item in partAttachPoints["types"] as Godot.Collections.Array)
        {
          types.Add(Parts.PartTypeConversion.FromString(item as string));
        }
        partBP.partAttachPoints.Add(new Parts.AttachPoint(new Vector2(x,y),types));
      }

      //Generate bitmap from texture data
      BitMap newBMP = new BitMap();

      newBMP.CreateFromImageAlpha(partBP.texture.GetData());
      partBP.bitMask = newBMP;

      //Add to the GRAND parts dictionary
      allPartsDict[partBP.partType].Add(partBP);
    }
  }

  //Load the Blueprints from the json file into the blueprints dictionary
  void LoadBlueprints()
  {

    //Read json file into text
    Godot.File file = new Godot.File();
    file.Open("res://Data/Blueprints.json", Godot.File.ModeFlags.Read);
    string jsonText = file.GetAsText();
    file.Close();

    //Construct dict of stuff
    Godot.Collections.Array ParsedData = Godot.JSON.Parse(jsonText).Result as Godot.Collections.Array;

    //Parse data based on Resource
    foreach (Godot.Collections.Dictionary data in ParsedData)
    {
      BaseBlueprint partBP = new BaseBlueprint();
      partBP.name = data["blueprintName"] as string;
      partBP.texture = (Texture)GD.Load(data["blueprintIconSprite"] as string);
      
      foreach (Godot.Collections.Dictionary subData in data["blueprintRequiredPieces"] as Godot.Collections.Array)
      {
        partBP.requiredPieces.Add(Parts.PartTypeConversion.FromString(subData["partType"] as string));
      }
      blueprints.Add(partBP.name, partBP);
    }
  }

  //Generate a callback button from a weapon blueprint piece
  CallbackTextureButton CreateCallbackButtonFromBlueprint(Parts.PartBlueprint blueprint, BasicCallback callback, Vector2 size, bool useBitmask = false, bool useColors = true, bool setMinSize = false)
  {
    //Generate individual part buttons
    CallbackTextureButton BPPieceButton = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
    //Generate a unique name for the part
    BPPieceButton.Name = blueprint.name + partNum++;
    
    //Set the size of the rect and need this stuff to get it to expand
    BPPieceButton.RectSize = blueprint.texture.GetSize();  //size of tex
    BPPieceButton.RectScale = size;   //new scale
    BPPieceButton.Expand = true;
    //BPPieceButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
    //
    if(setMinSize)
      BPPieceButton.RectMinSize = BPPieceButton.RectSize * BPPieceButton.RectScale;
    else
      BPPieceButton.RectMinSize = BPPieceButton.RectSize;
    
    //BPPieceButton.RectPosition -= BPPieceButton.RectSize * BPPieceButton.RectScale / 2.0f;

    //Set textures and bitmasks to the default part's texture and its bitmask
    BPPieceButton.TextureNormal = blueprint.texture;
    
    BPPieceButton.onButtonPressedCallback = callback;
    BPPieceButton.changeColors = useColors;
    BPPieceButton.Modulate = BPPieceButton.defaultColor;
  
    if(useBitmask)
      BPPieceButton.TextureClickMask = blueprint.bitMask;
    
    return BPPieceButton;
  }

  //Create the callback button from an attachment point on the weapon creation node 
  CallbackTextureButton CreateCallbackButtonFromAttachmentPoint(Parts.AttachPoint attachPoint, Parts.WeaponBlueprintNode node, BasicCallback callback, Vector2 partRectSize, bool useColors = true, bool setMinSize = false)
  {
    //Generate individual part buttons
    CallbackTextureButton newAttachpoint = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
    //Generate a unique name for the part
    
    //Set the size of the rect and need this stuff to get it to expand
    newAttachpoint.RectSize = attachPointTex.GetSize();  //size of tex
    newAttachpoint.RectScale = partVisualizerScale;   //new scale
    newAttachpoint.Expand = true;
    //newAttachpoint.StretchMode = TextureButton.StretchModeEnum.KeepAspect;

    if(setMinSize)
      newAttachpoint.RectMinSize = newAttachpoint.RectSize * newAttachpoint.RectScale;
    else
      newAttachpoint.RectMinSize = newAttachpoint.RectSize;

    //Set textures and bitmasks to the default part's texture and its bitmask
    newAttachpoint.TextureNormal = attachPointTex;
    
    newAttachpoint.RectPosition = (node.currentOffset + attachPoint.pos - (attachPointTex.GetSize() / 2.0f)) * partVisualizerScale;

    //TODO need to fix the usage of currentOffset for the 
    //if odd then move a bit
    //if(newAttachpoint.RectSize.x % 2 == 1 && (maxWeaponUIExtents.x-minWeaponUIExtents.x) % 2 == 0)
    //{
    //  newAttachpoint.RectPosition += new Vector2(0.5f * partVisualizerScale.x,0);
    //}
    //if(newAttachpoint.RectSize.y % 2 == 1 && (maxWeaponUIExtents.y-minWeaponUIExtents.y) % 2 == 0)
    //{
    //  newAttachpoint.RectPosition += new Vector2(0,0.5f * partVisualizerScale.y);
    //}

    //This fixed everything???
    newAttachpoint.RectPosition += new Vector2(0.5f * partVisualizerScale.x,0.5f * partVisualizerScale.y);

    
    newAttachpoint.onButtonPressedCallback = callback;
    newAttachpoint.changeColors = useColors;
    newAttachpoint.Modulate = new Color(0,0.8f,0,1);
    newAttachpoint.TextureClickMask = attachPointBitmask;
    
    return newAttachpoint;
  }

  //Clear the parts visualizer's children
  void ClearPartsVisualizer()
  {
    selectedPart = null;
    //Queue all current children to be deleted
    foreach (Node child in partVisualizerContainer.GetChildren())
    {
      partVisualizerContainer.RemoveChild(child);
      child.QueueFree();
    }
  }

  //Clear blueprint details UI
  void ClearCurrentBlueprintDetails()
  {
    foreach (Node child in currentBlueprintDetailContainer.GetChildren())
    {
      currentBlueprintDetailContainer.RemoveChild(child);
      child.QueueFree();
    }
  }

  //Clears the parts selection UI
  void ClearPartSelection()
  {
    //Queue all current children to be deleted
    foreach (Node child in newPartSelectionContainer.GetChildren())
    {
      if(child as HSeparator != null)
        continue;

      newPartSelectionContainer.RemoveChild(child);
      child.QueueFree();
    }
  }

  //Generate a new blueprint so the array can be moved into a FinishedBP list
  Parts.PartBlueprint CreatePartBlueprintFromType(Parts.PartType partType)
  {   
    return new Parts.PartBlueprint(allPartsDict[partType][0]);
  }

  //Generate a blueprint button from
  void GenerateBlueprintButton(BaseBlueprint loadedBP)
  {

    //Configure Button
    CallbackTextureButton newCallbackTextureButton = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
    newCallbackTextureButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
    newCallbackTextureButton.TextureNormal = loadedBP.texture;
    newCallbackTextureButton.blueprint = loadedBP.name;
    newCallbackTextureButton.SetSize(new Vector2(5,5));
    newCallbackTextureButton.changeColors = true;

    //Setup on pressed callback func
    newCallbackTextureButton.onButtonPressedCallback = () =>
    {
      //if we are selecting the same BP that we already have selected then break early
      if(selectedBlueprint == blueprints[newCallbackTextureButton.blueprint])
        return;
      //Update selected blueprint and the selected BP stuff
      selectedBlueprint = blueprints[newCallbackTextureButton.blueprint]; 
      currentBlueprintText.BbcodeText = fontBBcodePrefix + selectedBlueprint.name;

      //Clear current parts
      currentParts.Clear();
      //Add new piece icons
      foreach (var part in selectedBlueprint.requiredPieces)
      {
        currentParts.Add(allPartsDict[part][0]);//CreatePartBlueprintFromType(part));
      }
      currentParts.Sort(Parts.PartTypeConversion.CompareParts);
      
      //Clear part selection as well
      ClearPartSelection();
      GeneratePartVisualizerUIFromCurrentParts();
    };

    //Load the icons
    blueprintContainer.AddChild(newCallbackTextureButton);
  }
  
  public void UpdateCurrentlySelectedPart(CallbackTextureButton newSelectedPart)
  {
    if(newSelectedPart != null)
    {
      newSelectedPart.Modulate = newSelectedPart.pressedColor;
      newSelectedPart.changeColors = false;
    }
    if(selectedPart != null)
    {
      selectedPart.Modulate = selectedPart.defaultColor;
      selectedPart.changeColors = true;
    }
    selectedPart = newSelectedPart;
  }

  void UpdateCurrentlySelectedAttachPoint(Parts.AttachPoint attachPoint, CallbackTextureButton newAttachPointButton)
  {
    if(attachPoint != null && newAttachPointButton != null)
    {
      newAttachPointButton.Modulate = new Color(0,1,0);
      newAttachPointButton.changeColors = false;
    }
    if(selectedAttachPoint != null && newAttachPointButton != null)
    {
      newAttachPointButton.Modulate = new Color(1,1,1);
      newAttachPointButton.changeColors = true;
    }
    selectedAttachPoint = attachPoint;
  }

  void SetSelectedAttachPoint(Parts.AttachPoint attachPoint, CallbackTextureButton newAttachPointButton, Parts.WeaponBlueprintNode node)
  {
    //Reset selected part
    UpdateCurrentlySelectedPart(null);
    selectedWeaponBPNode = null;
    UpdateCurrentlySelectedAttachPoint(attachPoint, newAttachPointButton);
    selectedAttachPoint = attachPoint;
    ClearPartSelection();
    if(currentMode == CraftingSystemMode.PartSelection)
    {
      LoadPartSelectionAttachPoint(attachPoint, node, newAttachPointButton);
    }
    else
    {
      //Material Selection
    }
  }

  void GenerateAttachPointsUIFromPart(Parts.WeaponBlueprintNode node, Vector2 partRectSize)
  {
    foreach (var attachPoint in node.part.partAttachPoints)
    {
      //Only generate a new button if there is an open attachment slot
      if(attachPoint.attachedPart == false)
      {
        //Generate green attach point
        CallbackTextureButton newAttachPointButton = default;
        newAttachPointButton = CreateCallbackButtonFromAttachmentPoint(attachPoint, node,() => {SetSelectedAttachPoint(attachPoint, newAttachPointButton, node);}, partRectSize, true, false);

        partVisualizerContainer.AddChild(newAttachPointButton);
        //Set callback to SetSelectedAttachPoint
      }
    }
  }
  
  void GeneratePartsFromWeaponBPNode(Parts.WeaponBlueprintNode node, Vector2 baseOffset)
  {
    //Loads the part visualizer with callbacks to load the part selections
    //cannot pass BPPieceButton to the functor so need to initialize it to an object. 
    CallbackTextureButton BPPieceButton = default;
    BPPieceButton = CreateCallbackButtonFromBlueprint(node.part, () => 
    {
      if(BPPieceButton != selectedPart)
      {
        UpdateCurrentlySelectedPart(BPPieceButton);
        selectedWeaponBPNode = node;
        UpdateCurrentlySelectedAttachPoint(null, null);
        ClearPartSelection();
        if(currentMode == CraftingSystemMode.PartSelection)
        {
          LoadPartSelection(node);
        }
        else
        {
          //Set material to Undefined
          selectedInventoryMaterial = Materials.Material.Undefined;
          //Tell player to Select Material
          //Material Selection
        }
      }
    }, partVisualizerScale, true, true, false);

    
    //places the location - the attach point because attachpoint is -vector to move image so its 0,0 is the attach pt + the base offset
    node.currentOffset = -node.part.baseAttachPoint + baseOffset;
    BPPieceButton.RectPosition = node.currentOffset * partVisualizerScale;

    //if odd then move a bit
    //Weirdly this works perfectly with the attachment points but makes the problem worse here
    //if(BPPieceButton.RectSize.x % 2 == 1)
    //{
    //  BPPieceButton.RectPosition += new Vector2(0.5f * partVisualizerScale.x,0);
    //}
    //if(BPPieceButton.RectSize.y % 2 == 1)
    //{
    //  BPPieceButton.RectPosition += new Vector2(0,0.5f * partVisualizerScale.y);
    //}

    Parts.WeaponBlueprintNode parentNode = node.parent;

    //If not undefined than don't change the color
    if(currentMode == CraftingSystemMode.PartSelection)
    {
      BPPieceButton.Modulate = Materials.MaterialTints.tints[node.part.currentMaterial];
    }

    //If not undefined than don't change
    if(currentMode == CraftingSystemMode.MaterialSelection)
    {
      BPPieceButton.Modulate = Materials.MaterialTints.tints[node.part.currentMaterial];
    }

    partVisualizerContainer.AddChild(BPPieceButton);


    //Don't place attachment points in the material selection
    if(currentMode == CraftingSystemMode.PartSelection)
    {
      GenerateAttachPointsUIFromPart(node, BPPieceButton.RectSize * BPPieceButton.RectScale / 2.0f);
    }

    foreach (var item in node.children)
    {
      //- attach pt as its inverted
      GeneratePartsFromWeaponBPNode(item.Value, -node.part.baseAttachPoint + item.Key.pos + baseOffset);
    }
  }

  Vector2 maxWeaponUIExtents = Vector2.Zero;
  Vector2 minWeaponUIExtents = Vector2.Inf;

  //recursively get the largest UI extents
  void GetLargestUIExtents(Parts.WeaponBlueprintNode node)
  {
    //Set the bounding box of the weapon so we can rescale the UI
    maxWeaponUIExtents = new Vector2(Mathf.Max(node.currentOffset.x + node.part.texture.GetSize().x, maxWeaponUIExtents.x), Mathf.Max(node.currentOffset.y + node.part.texture.GetSize().y, maxWeaponUIExtents.y));
    minWeaponUIExtents = new Vector2(Mathf.Min(node.currentOffset.x - node.part.texture.GetSize().x, minWeaponUIExtents.x), Mathf.Min(node.currentOffset.y - node.part.texture.GetSize().y, minWeaponUIExtents.y));
    foreach (var item in node.children)
    {
      GetLargestUIExtents(item.Value);
    }   
  }

  void AccumulateStats(Parts.WeaponBlueprintNode node, ref Parts.PartStats summationStats)
  {
    summationStats = Parts.PartStats.GetCombinationOfStats(summationStats, node.part.stats);
    foreach (var item in node.children)
    {
      AccumulateStats(item.Value, ref summationStats);
    }   
  }

  void GeneratePartVisualizerUIFromCurrentParts()
  {
    maxWeaponUIExtents = Vector2.Zero;  
    minWeaponUIExtents = Vector2.Inf;

    //We need to update the currentOffset before we get the extents
    GeneratePartsFromWeaponBPNode(weaponRootNode, -weaponRootNode.part.texture.GetSize() / 2.0f);
    GetLargestUIExtents(weaponRootNode);

    ClearPartsVisualizer();
    ClearCurrentBlueprintDetails();

    Parts.PartStats summationStats = new Parts.PartStats();

    Console.WriteLine("Max Weapon UI Extents Part Visualizer Scale is " + maxWeaponUIExtents.ToString());
    Console.WriteLine("Min Weapon UI Extents Part Visualizer Scale is " + minWeaponUIExtents.ToString());
    
    //max - min = dist
    Vector2 weaponUIExtents = maxWeaponUIExtents - minWeaponUIExtents;
    Console.WriteLine("Weapon UI Extents Part Visualizer Scale is " + weaponUIExtents.ToString());

    //get the larget axis, MaxAxis returns the largest axis not the number of it
    float weaponUIMaxScale = Mathf.Max(weaponUIExtents.x, weaponUIExtents.y);

    Console.WriteLine("Weapon UI Extents Scale is " + weaponUIMaxScale.ToString());
    //hardcoded expected 32 to be the largest size so divide the max by the current to get the multiplier * the scale at 32 length gives us the new scalar (instead of 4)
    //And round it so that we don't have any float shenannagins 
    float newScale = Mathf.Round((32.0f/weaponUIMaxScale) * basePartVisualizerScale * 100.0f)/100.0f;

    partVisualizerScale = new Vector2(newScale,newScale);

    Console.WriteLine("New Part Visualizer Scale is " + partVisualizerScale.x.ToString());
    GeneratePartsFromWeaponBPNode(weaponRootNode, -weaponRootNode.part.texture.GetSize() / 2.0f);
    //GeneratePartsFromWeaponBPNode(weaponRootNode, -weaponRootNode.part.texture.GetSize() / 2.0f + (weaponUIExtents / 2.0f) / partVisualizerScale);
    
    AccumulateStats(weaponRootNode, ref summationStats);

    RichTextLabel bpDetails = new RichTextLabel();
    currentBlueprintDetailContainer.AddChild(bpDetails);
    bpDetails.BbcodeEnabled = true;

    //Only write text if we have parts
    //if(currentParts.Count >= 1)
    bpDetails.BbcodeText = summationStats.GenerateStatText(null, 0, false);
    
    bpDetails.RectMinSize = new Vector2(32,50);
    bpDetails.RectClipContent = false;
  }

  public override void _Draw()
  {

    Vector2 center = (partVisualizerContainer as Control).RectGlobalPosition;// + (partVisualizerContainer as Control).RectSize * 0.5f;
    //Vector2 center = (partVisualizerContainer as Control).RectGlobalPosition + (partVisualizerContainer as Control).RectSize * 0.5f;

    //Debug lines
    //DrawLine(center + minWeaponUIExtents, center + minWeaponUIExtents + new Vector2(0,100),new Color("fc0303"),2);  //Pos Y
    //DrawLine(center + maxWeaponUIExtents, center + maxWeaponUIExtents + new Vector2(0,-100),new Color("fcdb03"),2);  //Neg Y
    //DrawLine(center + minWeaponUIExtents, center + minWeaponUIExtents + new Vector2(100,0),new Color("0345fc"),2);  //Pos X
    //DrawLine(center + maxWeaponUIExtents, center + maxWeaponUIExtents + new Vector2(-100,0),new Color("fc03ce"),2);  //Neg X
  }
  

  public override void _Ready()
  {
    //TODO: Hardcode path so no search
    partVisualizerContainer = FindNode("PartsVisualizerContainer");
    currentBlueprintDetailContainer = FindNode("PartDetailContainer");
    blueprintContainer = FindNode("GridBlueprints") as HBoxContainer;
    newPartSelectionContainer = FindNode("NewPartSelectionContainer");
    currentBlueprintText = FindNode("CurrentBPTitle") as RichTextLabel;
    inventoryOres = FindNode("OreInventoryGridContainer") as GridContainer;

    LoadAllParts();
    //Start the current part as a empty handle
    currentParts.Add(allPartsDict[Parts.PartType.Handle][0]);
    weaponRootNode.part = allPartsDict[Parts.PartType.Handle][0];

    Color ironBaseColor = new Color("e8e8e8");
    //Materials.MaterialTints.tints = data from file
    //Pieces = data from file
    //genuinely using hex str to int lmaoo

    //For each BP in BP folder, load them
    Directory blueprintDir = new Directory();
    //Set dir to BP folder

    Directory spriteDir = new Directory();
    
    //Load sprites for bp's
    if(spriteDir.Open(FullSpriteDir) != Error.Ok)
    {
      throw(new Exception("Yo shit broke loading BP sprite Icons"));
    }

    //Load blueprint resources 
    foreach (var blueprint in blueprints)
    {
      GenerateBlueprintButton(blueprint.Value);
    }

    //Set initial state to MaterialSelection
    SetModePartSelection();
  }
  
  
  public override void _Process(float delta)
  {
    //Call update which calls _Draw
    Update();
  }

  //Updates dict with material cost
  public void GetWeaponMaterialCost(Inventory playerInventory)
  {
    Dictionary<Materials.Material, int> costOfWeapon = new Dictionary<Materials.Material, int>();
    List<string> partsMissingMaterials = new List<string>();
    if(IsWeaponsMaterialsSelected(weaponRootNode, ref partsMissingMaterials))
    {
      costOfWeapon = GetCostOfWeaponNode(weaponRootNode);
    }
    else
    {
      string partsMissingMaterialsString = string.Join(string.Empty, partsMissingMaterials);
      GetNode<RichTextLabel>("ParchmentBackground/CurrentWeaponInfo").Text = "Select Materials for Parts: " + partsMissingMaterialsString;
      costOfWeapon = null;
    }

    string missingMaterials = string.Empty;

    if(costOfWeapon != null)
    { 
      playerCanCraftWeapon = true;
      //Check playerInventory against items
      foreach (var item in costOfWeapon)
      {
        if(!playerInventory.HasMaterial(item.Key, item.Value))
        {
          missingMaterials += "Missing " + (item.Value - playerInventory.GetMaterialCount(item.Key)).ToString() + " pieces of " + item.Key.ToString() + "\n";
          playerCanCraftWeapon = false;
        }
      }
      GetNode<RichTextLabel>("ParchmentBackground/CurrentWeaponInfo").Text = missingMaterials;
    }

    GetNode<Button>("CraftButton").Disabled = !playerCanCraftWeapon;
  }

  //Simply recursively check if the weapon has its material selected
  public bool IsWeaponsMaterialsSelected(Parts.WeaponBlueprintNode node, ref List<string> partsMissingMaterials)
  {
    bool materialsSelected = true;
    
    if(node.part.currentMaterial == Materials.Material.Undefined)
    {
      materialsSelected = false;
      partsMissingMaterials.Add(node.part.name);
    }

    foreach(var child in node.children)
    {
      if(IsWeaponsMaterialsSelected(child.Value, ref partsMissingMaterials) == false)
        materialsSelected = false;
    }
    return materialsSelected;
  }

  public Dictionary<Materials.Material, int> GetCostOfWeaponNode(Parts.WeaponBlueprintNode node)
  {
    Dictionary<Materials.Material, int> currentCost = new Dictionary<Materials.Material, int>();

    currentCost.Add(node.part.currentMaterial, node.part.materialCost);

    foreach(var child in node.children)
    {
      Dictionary<Materials.Material, int> childCost = GetCostOfWeaponNode(child.Value);

      //Combine dicts
      foreach (var childVal in childCost)
      {
        int currVal = 0;
        if(currentCost.TryGetValue(childVal.Key, out currVal))
        {
          currentCost[childVal.Key] += childVal.Value;
        }
        else
        {
          currentCost.Add(childVal.Key, childVal.Value);
        }
      }
    }
    return currentCost;
  }

  public static int GetMinYSizeFromRichTextLabel(RichTextLabel label)
  {
    //min size is num lines * font size + spacings
    return (1 + label.BbcodeText.Count("\n")) * ((label.Theme.DefaultFont as DynamicFont).Size + (label.Theme.DefaultFont as DynamicFont).ExtraSpacingBottom + (label.Theme.DefaultFont as DynamicFont).ExtraSpacingTop);
  }

  //Loads the list of all possible parts of the passed part blueprint
  public void LoadPartSelection(Parts.WeaponBlueprintNode currentNode)
  {
    //Load all parts of this type
    foreach (var part in allPartsDict[currentNode.part.partType])
    {
      //Load part as clickable button with callback to set the current piece of the current blueprint as this piece
      CallbackTextureButton partSelectionButton = CreateCallbackButtonFromBlueprint(part, () => 
      {
        ClearPartSelection();
        currentNode.IterateNode((currNode) => 
        {
          foreach (var attachPt in currNode.part.partAttachPoints)
          {
              attachPt.attachedPart = false;
          }  
        });

        currentNode.ClearNodeChildren();
        currentNode.part = part;
        GeneratePartVisualizerUIFromCurrentParts();
      }, new Vector2(1,1), false, true, true);
      
      partSelectionButton.Modulate = partSelectionButton.defaultColor;

      /////////////////////////////////////////////////////////////////////////////////////////////////
      //Generate Detail Sprites
      HBoxContainer hBox = CallbackTextureButtonWithTextScene.Instance() as HBoxContainer;

      Node partIconParentNode = hBox.GetNode("VBoxContainer/HSplitContainer");
      Node node = partIconParentNode.GetNode("PartIcon");
      partIconParentNode.RemoveChild(partIconParentNode.GetNode("PartIcon"));     //Remove current selection button
      node.QueueFree();                       //Free node
      partIconParentNode.AddChild(partSelectionButton);     //add constructed obj
      partIconParentNode.MoveChild(partSelectionButton,0);  //move to pos 0

      partSelectionButton.RectMinSize = MinPartSelectionSize;

      RichTextLabel detailText = hBox.GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData") as RichTextLabel;
      detailText.BbcodeText = part.stats.GenerateStatText(currentNode.part);
      detailText.BbcodeEnabled = true;
      detailText.RectMinSize = new Vector2(detailText.RectMinSize.x,GetMinYSizeFromRichTextLabel(detailText));
      //Dont change colors with the callbacks
      newPartSelectionContainer.AddChild(hBox);
    }
  }

  //Loads the list of all possible parts of the passed attachment point
  public void LoadPartSelectionAttachPoint(Parts.AttachPoint attachPoint, Parts.WeaponBlueprintNode parentNode, CallbackTextureButton newAttachPointButton)
  {
    foreach (var partType in attachPoint.partTypes)
    {
      //Load all parts of this type
      foreach (var part in allPartsDict[partType])
      {
        //Load part as clickable button with callback to set the current piece of the current blueprint as this piece
        CallbackTextureButton partSelectionButton = CreateCallbackButtonFromBlueprint(part, () => 
        {
          ClearPartSelection();
          attachPoint.attachedPart = true;
          partVisualizerContainer.RemoveChild(newAttachPointButton);
          //Set the x/y pos of the attach point to the actual node that represents the part
          Parts.WeaponBlueprintNode newNode = new Parts.WeaponBlueprintNode(part, parentNode);
          parentNode.children.Add(attachPoint, newNode); 
          GeneratePartVisualizerUIFromCurrentParts(); 
        }, new Vector2(1,1), false, true, false); 

        partSelectionButton.Modulate = partSelectionButton.defaultColor;

        /////////////////////////////////////////////////////////////////////////////////////////////////
        //Generate Detail Sprites
        HBoxContainer hBox = CallbackTextureButtonWithTextScene.Instance() as HBoxContainer;

        Node partIconParentNode = hBox.GetNode("VBoxContainer/HSplitContainer");
        Node node = partIconParentNode.GetNode("PartIcon");
        partIconParentNode.RemoveChild(partIconParentNode.GetNode("PartIcon"));     //Remove current selection button
        node.QueueFree();                       //Free node
        partIconParentNode.AddChild(partSelectionButton);     //add constructed obj
        partIconParentNode.MoveChild(partSelectionButton,0);  //move to pos 0

        partSelectionButton.RectMinSize = MinPartSelectionSize;

        RichTextLabel detailText = hBox.GetNode<RichTextLabel>("VBoxContainer/HSplitContainer/PartData") as RichTextLabel;
        detailText.BbcodeText = part.stats.GenerateStatText();
        detailText.BbcodeEnabled = true;
        detailText.RectMinSize = new Vector2(detailText.RectMinSize.x, GetMinYSizeFromRichTextLabel(detailText));
        //Dont change colors with the callbacks
        newPartSelectionContainer.AddChild(hBox);
      }
    }
  }
}
