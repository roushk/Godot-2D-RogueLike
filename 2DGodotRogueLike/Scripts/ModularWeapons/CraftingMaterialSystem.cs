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

    RichTextLabel currentBlueprintText;

    string fontBBcodePrefix = "[center][b]";

    Node partVisualizerContainer;
    Node currentBlueprintDetailContainer;
    Node blueprintContainer;
    Node newPartSelectionContainer;

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

    CallbackTextureButton CreateCallbackButton(Parts.PartBlueprint blueprint, BasicCallback callback, Vector2 size, bool useBitmask = false, bool useColors = true)
    {
        //Generate individual part buttons
        CallbackTextureButton BPPieceButton = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
        //Generate a unique name for the part
        BPPieceButton.Name = blueprint.name + partNum++;
        
        //Set the size of the rect and need this stuff to get it to expand
        BPPieceButton.RectSize = new Vector2(32,32);  //size of tex
        BPPieceButton.RectScale = size;   //new scale
        BPPieceButton.Expand = true;
        BPPieceButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
        BPPieceButton.RectMinSize = BPPieceButton.RectSize * BPPieceButton.RectScale;

        //Set textures and bitmasks to the default part's texture and its bitmask
        BPPieceButton.TextureNormal = blueprint.texture;
        BPPieceButton.onButtonPressedCallback = callback;
        BPPieceButton.changeColors = useColors;
        BPPieceButton.Modulate = new Color(1,1,1,1);
    
        if(useBitmask)
            BPPieceButton.TextureClickMask = blueprint.bitMask;
        
        return BPPieceButton;
    }

    void ClearPartsVisualizer()
    {
        //Queue all current children to be deleted
        foreach (Node child in partVisualizerContainer.GetChildren())
        {
            partVisualizerContainer.RemoveChild(child);
            child.QueueFree();
        }
    }

    void ClearCurrentBlueprintDetails()
    {
        foreach (Node child in currentBlueprintDetailContainer.GetChildren())
        {
            currentBlueprintDetailContainer.RemoveChild(child);
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

    void GeneratePartVisualizerUIFromCurrentParts()
    {
        ClearPartsVisualizer();
        ClearCurrentBlueprintDetails();

        Parts.PartStats summationStats = new Parts.PartStats();
        foreach (var part in currentParts)
        {
            //Loads the part visualizer with callbacks to load the part selections
            CallbackTextureButton BPPieceButton = CreateCallbackButton(part, () => 
                {
                    LoadPartSelection(part);
                }, new Vector2(4,4),true, true);

            BPPieceButton.Modulate = new Color(1,1,1,0.4f);

            partVisualizerContainer.AddChild(BPPieceButton);

            /////////////////////////////////////////////////////////////////////////////////////////////////
           ////Generate Detail Sprites
           //HBoxContainer hBox = BPPartDetailScene.Instance() as HBoxContainer;
           //
           //currentBlueprintDetailContainer.AddChild(hBox);

           //CallbackTextureButton texDetail = hBox.GetChild(0) as CallbackTextureButton;
           ////Generate a unique name for the part
           //texDetail.Name = part.name + partNum;
           //texDetail.TextureNormal = part.texture;
           //texDetail.Modulate = new Color(1,1,1,1);
           //
           ////Set the size of the rect and need this stuff to get it to expand
           //texDetail.RectSize = new Vector2(32,32);  //size of tex
           ////texDetail.RectScale = new Vector2(1,1);   //new scale
           ////texDetail.Expand = true;
           //texDetail.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
           //texDetail.RectMinSize = texDetail.RectSize * texDetail.RectScale;

           //RichTextLabel detailText = hBox.GetChild(1) as RichTextLabel;
           //detailText.BbcodeText = part.stats.GenerateStatText();
           //detailText.BbcodeEnabled = true;
        }
        if(currentParts.Count >= 2)
        {
            summationStats = Parts.PartStats.GetCombinationOfStats(currentParts[0].stats, currentParts[1].stats);
            for (int i = 2; i < currentParts.Count; i++)
            {
                summationStats = Parts.PartStats.GetCombinationOfStats(summationStats, currentParts[i].stats);
            }
        }
        else if(currentParts.Count == 1)
        {
            summationStats = currentParts[0].stats;
        }


        RichTextLabel bpDetails = new RichTextLabel();
        currentBlueprintDetailContainer.AddChild(bpDetails);
        bpDetails.BbcodeEnabled = true;

        //Only write text if we have parts
        if(currentParts.Count >= 1)
            bpDetails.BbcodeText = summationStats.GenerateStatText(0, false);
        
        bpDetails.RectMinSize = new Vector2(110,200);
        bpDetails.RectClipContent = false;
    }

    public override void _Ready()
    {
        //TODO: Hardcode path so no search
        partVisualizerContainer = FindNode("PartsVisualizerContainer");
        currentBlueprintDetailContainer = FindNode("PartDetailContainer");
        blueprintContainer = FindNode("GridBlueprints") as HBoxContainer;
        newPartSelectionContainer = FindNode("NewPartSelectionContainer");
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
        GeneratePartVisualizerUIFromCurrentParts();
    }

    //Clears the parts selection UI
    void ClearPartSelection()
    {
        //Queue all current children to be deleted
        foreach (Node child in newPartSelectionContainer.GetChildren())
        {
            newPartSelectionContainer.RemoveChild(child);
            child.QueueFree();
        }
    }

    int GetMinYSizeFromRichTextLabel(RichTextLabel label)
    {
        //min size is num lines * font size + spacings
        return (1 + label.BbcodeText.Count("\n")) * ((label.Theme.DefaultFont as DynamicFont).Size + (label.Theme.DefaultFont as DynamicFont).ExtraSpacingBottom + (label.Theme.DefaultFont as DynamicFont).ExtraSpacingTop);
    }

    //Loads the list of all possible parts of the passed part blueprint
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
            }, new Vector2(1,1), false, true);
            
            partSelectionButton.Modulate = new Color(1,1,1,1);

            /////////////////////////////////////////////////////////////////////////////////////////////////
            //Generate Detail Sprites
            HBoxContainer hBox = BPPartDetailScene.Instance() as HBoxContainer;

            Node node = hBox.GetChild(0);
            hBox.RemoveChild(hBox.GetChild(0));     //Remove current selection button
            node.QueueFree();                       //Free node
            hBox.AddChild(partSelectionButton);     //add constructed obj
            hBox.MoveChild(partSelectionButton,0);  //move to pos 0

            RichTextLabel detailText = hBox.GetChild(1) as RichTextLabel;
            detailText.BbcodeText = part.stats.GenerateStatText();
            detailText.BbcodeEnabled = true;
            detailText.RectMinSize = new Vector2(detailText.RectMinSize.x,GetMinYSizeFromRichTextLabel(detailText));
            //Dont change colors with the callbacks
            newPartSelectionContainer.AddChild(hBox);
        }
    }
}
