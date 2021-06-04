using Godot;
using Godot.Collections;
using System;

public class CraftingMaterialSystem : Node
{
    //Signals

    public void SwitchToIronOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Iron];
    }

    public void SwitchToCopperOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Copper];
    }
    
    public void SwitchToTinOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Tin];
    }

    public void SwitchToAdamantiteOnButtonPressed()
    {
        ingot.Modulate = materialTints[Materials.Material.Adamantite];
    }

    //Dict of material tints to lookup of pieces
    Dictionary<Materials.Material, Color> materialTints = new Dictionary<Materials.Material, Color>();

    //Dict of type to list of pieces
    Dictionary<Parts.PartType, Array<Parts.PartBlueprint>> allPartsDict = new Dictionary<Parts.PartType, Array<Parts.PartBlueprint>>();

    System.Collections.Generic.List<Parts.PartBlueprint> currentParts = new System.Collections.Generic.List<Parts.PartBlueprint>();

    CallbackTextureButton ingot;

    BaseBlueprint selectedBlueprint;
    CallbackTextureButton selectedPart;

    Dictionary<string,BaseBlueprint> blueprints = new Dictionary<string,BaseBlueprint>();

    //Load packed scenes 
    PackedScene CallbackTextureButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/CallbackTextureButtonScene.tscn");
    PackedScene BPPartDetailScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/BPPartDetail.tscn");

    TextureRect blueprintIconUI;
    RichTextLabel currentBlueprintText;

    string fontBBcodePrefix = "[center][b]";

    Node currentPartContainer;
    Node partContainer;
    Node partDetailContainer;
    Node bpNode;
    Node newPartContainer;

    //Todo replace this with something significantly better..
    //It will help to change the type from TextureButton to derived class like the other BP thing with callbacks
    int partNum = 0;

    //Paths
    const string FullBPDir = "res://Data/Blueprints/";
    const string FullSpriteDir = "res://Assets/Art/My_Art/BlueprintIcons/";

    //Load parts into parts dictionary
    void LoadAllParts()
    {
        //https://www.c-sharpcorner.com/article/loop-through-enum-values-in-c-sharp/
        //For each Piece type generate an array in the pieces dict
        foreach (Parts.PartType type in Enum.GetValues(typeof(Parts.PartType)))
        { 
            if(type == Parts.PartType.Undefined)
                continue;
            allPartsDict[type] = new Array<Parts.PartBlueprint>();
        }

        Array<Parts.PartBlueprint> createdParts = new Array<Parts.PartBlueprint>();

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
            partBP.partType = Parts.PartTypeConversion.FromString(data["partType"] as string);
            
            //Don't ask
            partBP.materialCost = (int)(float)data["partCost"];
            //Ok, its because json uses floats only so object -> float -> int

            Godot.Collections.Dictionary partAttributes = data["partAttributes"] as Godot.Collections.Dictionary;
            partBP.stats.baseSlashDamage =  (int)(float)partAttributes["baseSlashDamage"];
            partBP.stats.baseStabDamage =   (int)(float)partAttributes["baseStabDamage"];
            partBP.stats.baseAttackSpeed =  (int)(float)partAttributes["baseAttackSpeed"];
            partBP.stats.baseSwingSpeed =   (int)(float)partAttributes["baseSwingSpeed"];
            partBP.stats.baseLength =       (int)(float)partAttributes["baseLength"];
            partBP.stats.specialStat =      partAttributes["specialStat"] as string;

            //Generate bitmap from texture data
            BitMap newBMP = new BitMap();
            newBMP.CreateFromImageAlpha(partBP.texture.GetData());
            partBP.bitMask = newBMP;

            //Add to the GRAND parts dictionary
            allPartsDict[partBP.partType].Add(partBP);
        }
    }

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

    CallbackTextureButton CreateCallbackButton(Parts.PartBlueprint blueprint, BasicCallback callback, Vector2 size, bool useBitmask = false)
    {
        //Generate individual part buttons
        CallbackTextureButton BPPieceButton = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
        //Generate a unique name for the part
        BPPieceButton.Name = blueprint.name + partNum++;
        
        //Set the size of the rect and need this stuff to get it to expand
        BPPieceButton.RectSize = new Vector2(32,32);  //size of tex
        BPPieceButton.RectScale = size;   //new scale
        BPPieceButton.Expand = true;
        BPPieceButton.StretchMode = TextureButton.StretchModeEnum.Scale;
        BPPieceButton.RectMinSize = BPPieceButton.RectSize * BPPieceButton.RectScale;

        //Set textures and bitmasks to the default part's texture and its bitmask
        BPPieceButton.TextureNormal = blueprint.texture;
        BPPieceButton.onButtonPressedCallback = callback;
    
        if(useBitmask)
            BPPieceButton.TextureClickMask = blueprint.bitMask;
        
        return BPPieceButton;
    }

    void ClearPartsVisualizer()
    {
        //Queue all current children to be deleted
        foreach (Node child in partContainer.GetChildren())
        {
            partContainer.RemoveChild(child);
            child.QueueFree();
        }
    }

    void ClearPartsDetails()
    {
        foreach (Node child in partDetailContainer.GetChildren())
        {
            partDetailContainer.RemoveChild(child);
            child.QueueFree();
        }
    }


    //Generate a new blueprint so the array can be moved into a FinishedBP list
    Parts.PartBlueprint CreatePartBlueprintFromType(Parts.PartType partType)
    {   
        return new Parts.PartBlueprint(allPartsDict[partType][0]);
    }

    void GenerateBlueprintButton(BaseBlueprint loadedBP)
    {

        //Configure Button
        CallbackTextureButton newTexRect = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
        newTexRect.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        newTexRect.TextureNormal = loadedBP.texture;
        newTexRect.blueprint = loadedBP.name;
        newTexRect.SetSize(new Vector2(5,5));

        //Setup on pressed callback func
        newTexRect.onButtonPressedCallback = () =>
        {
            //if we are selecting the same BP that we already have selected then break early
            if(selectedBlueprint == blueprints[newTexRect.blueprint])
                return;
            //Update selected blueprint and the selected BP stuff
            selectedBlueprint = blueprints[newTexRect.blueprint]; 
            blueprintIconUI.Texture = selectedBlueprint.texture;
            currentBlueprintText.BbcodeText = fontBBcodePrefix + selectedBlueprint.name;

            //Clear current parts
            currentParts.Clear();
            //Add new piece icons
            foreach (var part in selectedBlueprint.requiredPieces)
            {
                currentParts.Add(CreatePartBlueprintFromType(part));
            }
            currentParts.Sort(Parts.PartTypeConversion.CompareParts);
            
            GeneratePartVisualizerUIFromCurrentParts();
        };

        //Load the icons
        bpNode.AddChild(newTexRect);
    }

    void GeneratePartVisualizerUIFromCurrentParts()
    {
        ClearPartsVisualizer();
        ClearPartsDetails();

        foreach (var part in currentParts)
        {
            //Loads the part visualizer with callbacks to load the part selections
            CallbackTextureButton BPPieceButton = CreateCallbackButton(part, () => 
                {
                    LoadPartSelection(part);
                }, new Vector2(4,4),true);

            BPPieceButton.Modulate = new Color(1,1,1,0.4f);

            partContainer.AddChild(BPPieceButton);

            /////////////////////////////////////////////////////////////////////////////////////////////////
            //Generate Detail Sprites
            HBoxContainer hBox = BPPartDetailScene.Instance() as HBoxContainer;

            partDetailContainer.AddChild(hBox);

            TextureRect texDetail = hBox.GetChild(0) as TextureRect;
            //Generate a unique name for the part
            texDetail.Name = part.name + partNum;
            texDetail.Texture = part.texture;
            
            //Set the size of the rect and need this stuff to get it to expand
            texDetail.RectSize = new Vector2(32,32);  //size of tex
            //texDetail.RectScale = new Vector2(1,1);   //new scale
            //texDetail.Expand = true;
            texDetail.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            texDetail.RectMinSize = texDetail.RectSize * texDetail.RectScale;

            RichTextLabel detailText = hBox.GetChild(1) as RichTextLabel;
            detailText.Text = "Correctly Set Detail Text, Very Cool";
        }
    }

    public override void _Ready()
    {
        currentPartContainer = FindNode("PartsVisualizerContainer");
        partContainer = FindNode("PartsVisualizerContainer");
        partDetailContainer = FindNode("PartDetailContainer");
        bpNode = FindNode("GridBlueprints") as HBoxContainer;
        newPartContainer = FindNode("NewPartDetailContainer");

        //Setup UI links
        ingot = GetNode("Ingot") as CallbackTextureButton;
        
        blueprintIconUI = FindNode("BlueprintIcon") as TextureRect;
        blueprintIconUI.SetSize(new Vector2(10,10));
        
        currentBlueprintText = FindNode("CurrentBPTitle") as RichTextLabel;

        LoadAllParts();
        LoadBlueprints();

        Color ironBaseColor = new Color("e8e8e8");
        //materialTints = data from file
        //Pieces = data from file
        //genuinely using hex str to int lmaoo
        materialTints[Materials.Material.Iron] =        new Color("e8e8e8");
        materialTints[Materials.Material.Copper] =      new Color("e8a25d");
        materialTints[Materials.Material.Tin] =         new Color("faf4dc");
        materialTints[Materials.Material.Bronze] =      new Color("e8c774");
        materialTints[Materials.Material.Steel] =       new Color("a2e8b7");
        materialTints[Materials.Material.Gold] =        new Color("e8dc5d");
        materialTints[Materials.Material.Platinum] =    new Color("e6f2ff");
        materialTints[Materials.Material.Adamantite] =  new Color("e86868");
        materialTints[Materials.Material.Mithril] =     new Color("a2e8b7");
        materialTints[Materials.Material.Cobalt] =      new Color("a2aee8");
        materialTints[Materials.Material.Darksteel] =   new Color("696969");
        materialTints[Materials.Material.Titanium] =    new Color("ffffff");

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
    }

    void ClearPartSelection()
    {
        //Queue all current children to be deleted
        foreach (Node child in newPartContainer.GetChildren())
        {
            newPartContainer.RemoveChild(child);
            child.QueueFree();
        }
    }

    //Loads the list of all possible parts of the passed part type for the node specified by partName
    public void LoadPartSelection(Parts.PartBlueprint currentBlueprint)
    {
        ClearPartSelection();

        //Load all parts of this type
        foreach (var part in allPartsDict[currentBlueprint.partType])
        {
            //Load part as clickable button with callback to set the current piece of the current blueprint as this piece
            CallbackTextureButton partSelectionButton = CreateCallbackButton(part, () => 
            {
                ClearPartSelection();
                currentParts.Remove(currentBlueprint);
                currentParts.Add(part);
                currentParts.Sort(Parts.PartTypeConversion.CompareParts);
                GeneratePartVisualizerUIFromCurrentParts();
            }, new Vector2(1,1), false);
            
            partSelectionButton.Modulate = new Color(1,1,1,1);

            //Dont change colors with the callbacks
            newPartContainer.AddChild(partSelectionButton);
        }
    }
}
