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

    BaseBlueprint currentblueprint = (BaseBlueprint)GD.Load("res://Data/Blueprints/BigAssSwordBP.tres");

    Dictionary<string,BaseBlueprint> blueprints = new Dictionary<string,BaseBlueprint>();

    //Load the BP Icon scene
    PackedScene BPiconScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/BlueprintIcon.tscn");

    TextureRect blueprintIconUI;

    public override void _Ready()
    {
        ingot = GetNode("Ingot") as BPTextureButton;
        blueprintIconUI = GetNode("PartsContainer/BlueprintIcon") as TextureRect;
        blueprintIconUI.SetSize(new Vector2(10,10));
        
        //https://www.c-sharpcorner.com/article/loop-through-enum-values-in-c-sharp/
        //For each Piece type generate an array in the pieces dict
        foreach (Pieces.PieceType type in Enum.GetValues(typeof(Pieces.PieceType)))  
        { 
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


        var bpNode = GetNode("Blueprints") as GridContainer;
        
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
                BPTextureButton newTexRect = BPiconScene.Instance() as BPTextureButton;
                newTexRect.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
                newTexRect.TextureNormal = loadedBPIcon;
                newTexRect.blueprint = loadedBP.name;
                //Setup on pressed callback
                newTexRect.onButtonPressedCallback = () =>{ currentblueprint = blueprints[newTexRect.blueprint];};

                newTexRect.SetSize(new Vector2(5,5));

                //Load the icons
                bpNode.AddChild(newTexRect);
                //Load the bp's
                blueprints.Add(loadedBP.name, loadedBP);

                
                
                //iterate next BP's
                nextBlueprint = blueprintDir.GetNext();
            }
        }
        else
        {
            throw(new Exception("Yo shit broke loading Blueprints"));
        }
        
        

        //For each blueprint
        foreach (var bp in blueprints)
        {
            //Create new node inside of the blueprints 
 
        }
    }

    public override void _Process(float delta)
    {
        blueprintIconUI.Texture = currentblueprint.iconTex;

        foreach (var bp in blueprints)
        {
            
        }
    }
}
