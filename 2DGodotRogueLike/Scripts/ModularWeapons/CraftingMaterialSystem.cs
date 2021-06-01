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
    Dictionary<Pieces.PieceType, Array<Pieces.BasePiece>> pieces = new Dictionary<Pieces.PieceType, Array<Pieces.BasePiece>>();
    
    BPTextureButton ingot;

    BaseBlueprint selectedBlueprint;
    Pieces.BasePiece selectedPiece;

    Dictionary<string,BaseBlueprint> blueprints = new Dictionary<string,BaseBlueprint>();

    Dictionary<Pieces.PieceType,Texture> pieceIcons = new  Dictionary<Pieces.PieceType,Texture>();

    //Todo: Consider creating Bitmasks for each type of piece and use it when constructing the menu instead of using the basic types???
    Dictionary<Pieces.PieceType,BitMap> pieceIconBitmasks = new  Dictionary<Pieces.PieceType,BitMap>();

    //Load packed scenes 
    PackedScene BPIconScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/BlueprintIcon.tscn");
    PackedScene BPPieceDataScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/BPPieceDetail.tscn");

    TextureRect blueprintIconUI;
    RichTextLabel currentBlueprintText;

    //TODO determine if we actually need these???
    Array<TextureButton> blueprintVisualPieces = new Array<TextureButton>();
    Array<HBoxContainer> blueprintDetailPieces = new Array<HBoxContainer>();


    //Todo replace this with something significantly better..
    //It will help to change the type from TextureButton to derived class like the other BP thing with callbacks
    
    public void UpdateCurrentlySelectedPieceNone()
    {
        if(blueprintVisualPieces.Count < 4)
            return;
        blueprintVisualPieces[0].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[1].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[2].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[3].Modulate = new Color(1,1,1,0.5f);   
    }

    public void UpdateCurrentlySelectedPiece1()
    {
        if(blueprintVisualPieces.Count < 4)
            return;
        blueprintVisualPieces[0].Modulate = new Color(1,1,1,1);
        blueprintVisualPieces[1].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[2].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[3].Modulate = new Color(1,1,1,0.5f);   
    }

    public void UpdateCurrentlySelectedPiece2()
    {
        if(blueprintVisualPieces.Count < 4)
            return;
        blueprintVisualPieces[1].Modulate = new Color(1,1,1,1);
        blueprintVisualPieces[0].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[2].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[3].Modulate = new Color(1,1,1,0.5f);
    }

    public void UpdateCurrentlySelectedPiece3()
    {
        if(blueprintVisualPieces.Count < 4)
            return;
        blueprintVisualPieces[2].Modulate = new Color(1,1,1,1);
        blueprintVisualPieces[1].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[0].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[3].Modulate = new Color(1,1,1,0.5f);
    }

    public void UpdateCurrentlySelectedPiece4()
    {
        if(blueprintVisualPieces.Count < 4)
            return;
        blueprintVisualPieces[3].Modulate = new Color(1,1,1,1);
        blueprintVisualPieces[1].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[2].Modulate = new Color(1,1,1,0.5f);
        blueprintVisualPieces[0].Modulate = new Color(1,1,1,0.5f);
    }

    public override void _Ready()
    {
        //Setup UI links
        ingot = GetNode("Ingot") as BPTextureButton;
        
        blueprintIconUI = FindNode("BlueprintIcon") as TextureRect;
        blueprintIconUI.SetSize(new Vector2(10,10));
        
        currentBlueprintText = FindNode("CurrentBPname") as RichTextLabel;

        //https://www.c-sharpcorner.com/article/loop-through-enum-values-in-c-sharp/
        //For each Piece type generate an array in the pieces dict
        foreach (Pieces.PieceType type in Enum.GetValues(typeof(Pieces.PieceType)))  
        { 
            if(type == Pieces.PieceType.Undefined)
                continue;
            pieces[type] = new Array<Pieces.BasePiece>();
        }

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

        var bpNode = FindNode("GridBlueprints") as GridContainer;
        
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
                BPTextureButton newTexRect = BPIconScene.Instance() as BPTextureButton;
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
                    
                    blueprintDetailPieces.Clear();
                    blueprintVisualPieces.Clear();

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
                    //Add new piece icons
                    foreach (var part in selectedBlueprint.requiredPieces)
                    {
                        //Generate Shadow
                        TextureButton tex = new TextureButton();
                        //Generate a unique name for the part
                        tex.Name = "Part_" + partNum++;
                        tex.TextureNormal = pieceIcons[part];
                        
                        //Set the size of the rect and need this stuff to get it to expand
                        tex.RectSize = new Vector2(32,32);  //size of tex
                        tex.RectScale = new Vector2(4,4);   //new scale
                        tex.Expand = true;
                        tex.StretchMode = TextureButton.StretchModeEnum.Scale;
                        tex.RectMinSize = tex.RectSize * tex.RectScale;
                        tex.TextureClickMask = pieceIconBitmasks[part];

                        //This works
                        tex.Connect("mouse_entered", this, "UpdateCurrentlySelectedPiece" + partNum.ToString());
                        tex.Connect("mouse_exited",this,"UpdateCurrentlySelectedPieceNone");
                        //Modulate to 0.5 alpha
                        tex.Modulate = new Color(1,1,1,0.5f);

                        partContainer.AddChild(tex);
                        blueprintVisualPieces.Add(tex);

                        //Generate Detail Sprites
                        HBoxContainer hBox = BPPieceDataScene.Instance() as HBoxContainer;

                        partDetailContainer.AddChild(hBox);
                        blueprintDetailPieces.Add(hBox);

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

                        tex.TextureHover = pieceIcons[part];
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
            foreach (Pieces.PieceType type in Enum.GetValues(typeof(Pieces.PieceType)))  
            { 
                if(type == Pieces.PieceType.Undefined)
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
        //For each blueprint
        foreach (var bp in blueprints)
        {
            //Create new node inside of the blueprints 
 
        }
    }

    public override void _Process(float delta)
    {            

        foreach (var bp in blueprints)
        {
            
        }
    }
}
