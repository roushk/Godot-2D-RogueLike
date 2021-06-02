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
    Dictionary<Parts.PartType, Array<Parts.PartBlueprint>> parts = new Dictionary<Parts.PartType, Array<Parts.PartBlueprint>>();
    
    CallbackTextureButton ingot;

    BaseBlueprint selectedBlueprint;
    CallbackTextureButton selectedPart;

    Dictionary<string,BaseBlueprint> blueprints = new Dictionary<string,BaseBlueprint>();

    Dictionary<Parts.PartType,Texture> pieceIcons = new  Dictionary<Parts.PartType,Texture>();

    //Todo: Consider creating Bitmasks for each type of piece and use it when constructing the menu instead of using the basic types???
    Dictionary<Parts.PartType,BitMap> pieceIconBitmasks = new  Dictionary<Parts.PartType,BitMap>();

    //Load packed scenes 
    PackedScene CallbackTextureButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/CallbackTextureButtonScene.tscn");
    PackedScene BPPartDetailScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/BPPartDetail.tscn");

    TextureRect blueprintIconUI;
    RichTextLabel currentBlueprintText;

    //TODO determine if we actually need these???
    Array<CallbackTextureButton> blueprintVisualParts = new Array<CallbackTextureButton>();
    Array<HBoxContainer> blueprintDetailParts = new Array<HBoxContainer>();

    //Todo replace this with something significantly better..
    //It will help to change the type from TextureButton to derived class like the other BP thing with callbacks
    
    //Load parts into parts dictionary
    void LoadAllParts()
    {
        //https://www.c-sharpcorner.com/article/loop-through-enum-values-in-c-sharp/
        //For each Piece type generate an array in the pieces dict
        foreach (Parts.PartType type in Enum.GetValues(typeof(Parts.PartType)))  
        { 
            if(type == Parts.PartType.Undefined)
                continue;
            parts[type] = new Array<Parts.PartBlueprint>();
        }

        Array<Parts.PartBlueprint> createdParts = new Array<Parts.PartBlueprint>();

        //Read json file into text
        Godot.File file = new Godot.File();
        file.Open("res://Data/PartsList.json", Godot.File.ModeFlags.Read);
        string jsonText = file.GetAsText();
        
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

            //Add to the GRAND parts dictionary
            parts[partBP.partType].Add(partBP);
        }
        file.Close();
    }

    public override void _Ready()
    {
        //Setup UI links
        ingot = GetNode("Ingot") as CallbackTextureButton;
        
        blueprintIconUI = FindNode("BlueprintIcon") as TextureRect;
        blueprintIconUI.SetSize(new Vector2(10,10));
        
        currentBlueprintText = FindNode("CurrentBPname") as RichTextLabel;

        //LoadAllParts();

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
        const string FullBPDir = "res://Data/Blueprints/";

        Directory spriteDir = new Directory();
        const string FullSpriteDir = "res://Assets/Art/My_Art/BlueprintIcons/";

        var bpNode = FindNode("GridBlueprints") as HBoxContainer;
        
        var partContainer = GetNode("PartsVisualizerContainer") as CenterContainer;
        var partDetailContainer = FindNode("PartDetailContainer") as VBoxContainer;

        //Load sprites for bp's
        if(spriteDir.Open(FullSpriteDir) != Error.Ok)
        {
            throw(new Exception("Yo shit broke loading BP sprite Icons"));
        }

        //Load blueprint resources 
        if(blueprintDir.Open(FullBPDir) == Error.Ok)
        {
            blueprintDir.ListDirBegin(true, true);

            //GetNext closes the Dir
            string nextBlueprint = blueprintDir.GetNext();

            //While not empty string
            while(nextBlueprint != "")
            {
                Console.WriteLine("Loading Blueprint \"" + FullBPDir + nextBlueprint + "\"");
                BaseBlueprint loadedBP = (BaseBlueprint)GD.Load(FullBPDir + nextBlueprint);
                
                //Load the icon for the BP + generate new texture
                Texture loadedBPIcon = (Texture)GD.Load(FullSpriteDir + loadedBP.iconSpriteName + ".png");
                loadedBP.iconTex = loadedBPIcon;

                //Configure Button
                CallbackTextureButton newTexRect = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
                newTexRect.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
                newTexRect.TextureNormal = loadedBPIcon;
                newTexRect.blueprint = loadedBP.name;

                //Setup on pressed callback func
                newTexRect.onButtonPressedCallback = () =>
                { 
                    //if we are selecting the same BP that we already have selected then break early
                    if(selectedBlueprint == blueprints[newTexRect.blueprint])
                        return;

                    selectedBlueprint = blueprints[newTexRect.blueprint]; 
                    blueprintIconUI.Texture = selectedBlueprint.iconTex;
                    currentBlueprintText.Text = selectedBlueprint.name;
                    
                    blueprintDetailParts.Clear();
                    blueprintVisualParts.Clear();

                    //Queue all current children to be deleted
                    foreach (Node child in partContainer.GetChildren())
                    {
                        partContainer.RemoveChild(child);
                        child.QueueFree();
                    }

                    foreach (Node child in partDetailContainer.GetChildren())
                    {
                        partDetailContainer.RemoveChild(child);
                        child.QueueFree();
                    }

                    int partNum = 0;

                    //Todo offload this loading from the callback to a startup and just toggle on/off the parts and change the name of their container instead of
                    //creating them inside of the callback from button pressed on the blueprint

                    //Add new piece icons
                    foreach (var part in selectedBlueprint.requiredPieces)
                    {
                        //Generate individual part buttons
                        CallbackTextureButton BPPieceButton = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
                        //Generate a unique name for the part
                        BPPieceButton.Name = "Part_" + partNum++;
                        
                        //Set the size of the rect and need this stuff to get it to expand
                        BPPieceButton.RectSize = new Vector2(32,32);  //size of tex
                        BPPieceButton.RectScale = new Vector2(4,4);   //new scale
                        BPPieceButton.Expand = true;
                        BPPieceButton.StretchMode = TextureButton.StretchModeEnum.Scale;
                        BPPieceButton.RectMinSize = BPPieceButton.RectSize * BPPieceButton.RectScale;

                        //Set textures and bitmasks
                        BPPieceButton.TextureNormal = pieceIcons[part];
                        BPPieceButton.TextureClickMask = pieceIconBitmasks[part];
                        
                        
                        //Added this recently?
                        BPPieceButton.TextureHover = pieceIcons[part];

                        BPPieceButton.onButtonPressedCallback = () => 
                        {
                            //On select of blueprint piece.
                            //LoadPartSelection(part, BPPieceButton.Name);
                        };
                        
                        //Dont change colors with the callbacks
                       // BPPieceButton.changeColors = false;

                        //Modulate to 0.25 alpha for unselected
                        BPPieceButton.Modulate = new Color(1,1,1,0.25f);

                        partContainer.AddChild(BPPieceButton);
                        blueprintVisualParts.Add(BPPieceButton);

                        /////////////////////////////////////////////////////////////////////////////////////////////////
                        //Generate Detail Sprites
                        HBoxContainer hBox = BPPartDetailScene.Instance() as HBoxContainer;

                        partDetailContainer.AddChild(hBox);
                        blueprintDetailParts.Add(hBox);

                        TextureRect texDetail = hBox.GetChild(0) as TextureRect;
                        //Generate a unique name for the part
                        texDetail.Name = "DetailPart_" + partNum;
                        texDetail.Texture = pieceIcons[part];
                        
                        //Set the size of the rect and need this stuff to get it to expand
                        texDetail.RectSize = new Vector2(32,32);  //size of tex
                        //texDetail.RectScale = new Vector2(1,1);   //new scale
                        //texDetail.Expand = true;
                        texDetail.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
                        texDetail.RectMinSize = texDetail.RectSize * texDetail.RectScale;

                        RichTextLabel detailText = hBox.GetChild(1) as RichTextLabel;
                        detailText.Text = "Correctly Set Detail Text, Very Cool";

                    }
                };

                newTexRect.SetSize(new Vector2(5,5));

                //Load the icons
                bpNode.AddChild(newTexRect);
                //Load the bp's
                blueprints.Add(loadedBP.name, loadedBP);
                
                //iterate next BP's
                nextBlueprint = blueprintDir.GetNext();
            }

            const string PartSpriteDir = "res://Assets/Art/My_Art/PartIcons/";
            foreach (Parts.PartType type in Enum.GetValues(typeof(Parts.PartType)))  
            { 
                if(type == Parts.PartType.Undefined)
                    continue;
                //Standard is load tex from file, no easy way to load image to tex that I can see and docs suggest loading with the Load func instead of creating it with an Image
                Texture newTex = (Texture)GD.Load(PartSpriteDir + type.ToString() + ".png");

                //Generate bitmap from texture data
                BitMap newBMP = new BitMap();
                newBMP.CreateFromImageAlpha(newTex.GetData());

                pieceIcons.Add(type,newTex);
                pieceIconBitmasks.Add(type, newBMP);
            }
        }
        else
        {
            throw(new Exception("Loading Blueprints Broke Blueprints"));
        }
    }

    public override void _Process(float delta)
    {            
        //if(selectedPart!=null)
        //    selectedPart.Modulate = new Color(1,1,1,1);
    }

    public void onHoverStartPartVisualizer()
    {

    }

    public void onHoverEndPartVisualizer()
    {

    }

    public void LoadPartSelection(Parts.PartType typeToLoad, string partName)
    {
        selectedPart = GetNode("PartsVisualizerContainer").GetNode(partName) as CallbackTextureButton;
        int partNum = 0;
        //Load all parts of this type
        foreach (var part in parts[typeToLoad])
        {
            //Load part as clickable button with callback to set the current piece of the current blueprint as this piece
            CallbackTextureButton PartSelectionButton = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
            //Generate a unique name for the part
            PartSelectionButton.Name = "Part_Selection_" + partNum++;
            
            //Set the size of the rect and need this stuff to get it to expand
            PartSelectionButton.RectSize = new Vector2(32,32);  //size of tex
            PartSelectionButton.RectScale = new Vector2(4,4);   //new scale
            PartSelectionButton.Expand = true;
            PartSelectionButton.StretchMode = TextureButton.StretchModeEnum.Scale;
            PartSelectionButton.RectMinSize = PartSelectionButton.RectSize * PartSelectionButton.RectScale;

            //Set textures and bitmasks
            PartSelectionButton.TextureNormal = pieceIcons[typeToLoad];
            PartSelectionButton.TextureClickMask = pieceIconBitmasks[typeToLoad];

            PartSelectionButton.onButtonPressedCallback = () => 
            {
                //On select of blueprint piece.
                ChangeSelectedPartTo(partName, PartSelectionButton.Name);
            };
            
            //Dont change colors with the callbacks
            //PartSelectionButton.changeColors = false;

            //Modulate to 0.25 alpha for unselected
            PartSelectionButton.Modulate = new Color(1,1,1,0.25f);
        }
    }

    void ChangeSelectedPartTo(string currentPartName, string newPartName)
    {

    }

    //CallbackTextureButton CreateCallbackTextureButtonFrom()
    //{
    //    
    //}
}
