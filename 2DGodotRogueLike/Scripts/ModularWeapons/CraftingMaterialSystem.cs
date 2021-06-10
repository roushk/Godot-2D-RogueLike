using Godot;
using Godot.Collections;
using System;

public class CraftingMaterialSystem : Control
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

	Parts.WeaponBlueprintNode baseNode = new Parts.WeaponBlueprintNode();

	System.Collections.Generic.List<Parts.AttachPoint> attachPoints = new System.Collections.Generic.List<Parts.AttachPoint>();

	CallbackTextureButton ingot;

	BaseBlueprint selectedBlueprint;
	CallbackTextureButton selectedPart;
	Parts.AttachPoint selectedAttachPoint;

	Dictionary<string,BaseBlueprint> blueprints = new Dictionary<string,BaseBlueprint>();

	//Load packed scenes 
	PackedScene CallbackTextureButtonScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/CallbackTextureButtonScene.tscn");
	PackedScene BPPartDetailScene = (PackedScene)ResourceLoader.Load("res://Scenes/BlueprintSystem/BPPartDetail.tscn");

	RichTextLabel currentBlueprintText;

	string fontBBcodePrefix = "[center][b]";

	Texture attachPointTex;
	BitMap attachPointBitmask;
	const string attachPointAssetStr = "res://Assets/Art/My_Art/AttachPoint.png";

	Node partVisualizerContainer;
	Node currentBlueprintDetailContainer;
	Node blueprintContainer;
	Node newPartSelectionContainer;

	Vector2 partVisualizerScale = new Vector2(4,4);

	//Todo replace this with something significantly better..
	//It will help to change the type from TextureButton to derived class like the other BP thing with callbacks
	int partNum = 0;

	//Paths
	const string FullBPDir = "res://Data/Blueprints/";
	const string FullSpriteDir = "res://Assets/Art/My_Art/BlueprintIcons/";

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
				Array<Parts.PartType> types = new Array<Parts.PartType>();
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
		BPPieceButton.StretchMode = TextureButton.StretchModeEnum.KeepAspectCentered;
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
		BPPieceButton.Modulate = new Color(1,1,1,1);
	
		if(useBitmask)
			BPPieceButton.TextureClickMask = blueprint.bitMask;
		
		return BPPieceButton;
	}

	CallbackTextureButton CreateCallbackButtonFromAttachmentPoint(Parts.AttachPoint attachPoint, BasicCallback callback, Vector2 partRectSize, bool useColors = true, bool setMinSize = false)
	{
		//Generate individual part buttons
		CallbackTextureButton newAttachpoint = CallbackTextureButtonScene.Instance() as CallbackTextureButton;
		//Generate a unique name for the part
		
		//Set the size of the rect and need this stuff to get it to expand
		newAttachpoint.RectSize = attachPointTex.GetSize();  //size of tex
		newAttachpoint.RectScale = partVisualizerScale;   //new scale
		newAttachpoint.Expand = true;
		newAttachpoint.StretchMode = TextureButton.StretchModeEnum.KeepAspect;
		newAttachpoint.AnchorLeft = 0.5f;
		newAttachpoint.AnchorTop = 0.5f;
		newAttachpoint.AnchorRight = 0.5f;
		newAttachpoint.AnchorBottom = 0.5f;
		
		if(setMinSize)
			newAttachpoint.RectMinSize = newAttachpoint.RectSize * newAttachpoint.RectScale;
		else
			newAttachpoint.RectMinSize = newAttachpoint.RectSize;

		//Center the object
		newAttachpoint.RectPosition += new Vector2((attachPoint.pos.x)*partVisualizerScale.x + 2,((attachPoint.pos.y)*partVisualizerScale.y) + 2) - ((newAttachpoint.RectSize * partVisualizerScale / 2.0f) + partRectSize);
		//newAttachpoint.RectPosition += (attachPoint.pos - ((newAttachpoint.RectSize + attachPointTex.GetSize()) / 2.0f)) * partVisualizerScale;
		//Set textures and bitmasks to the default part's texture and its bitmask
		newAttachpoint.TextureNormal = attachPointTex;
		
		newAttachpoint.onButtonPressedCallback = callback;
		newAttachpoint.changeColors = useColors;
		newAttachpoint.Modulate = new Color(0,0.8f,0,1);
		newAttachpoint.TextureClickMask = attachPointBitmask;
		
		return newAttachpoint;
	}

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

	//Clear the attachment point UI
	//void ClearAttachPoints()
	//{
	//    selectedAttachPoint = null;
	//    //Queue all current children to be deleted
	//    foreach (Node child in attachPointsContainer.GetChildren())
	//    {
	//        if(child as HSeparator != null)
	//            continue;
	//        attachPointsContainer.RemoveChild(child);
	//        child.QueueFree();
	//    }
	//}

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
	
	void UpdateCurrentlySelectedPart(CallbackTextureButton newSelectedPart)
	{
		if(newSelectedPart != null)
		{
			newSelectedPart.Modulate = new Color(0,1,0);
			newSelectedPart.changeColors = false;
		}
		if(selectedPart != null)
		{
			selectedPart.Modulate = new Color(1,1,1);
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
		UpdateCurrentlySelectedAttachPoint(attachPoint, newAttachPointButton);
		selectedAttachPoint = attachPoint;
		ClearPartSelection();
		LoadPartSelectionAttachPoint(attachPoint,node);
	}

	void GenerateAttachPointsUIFromPart(Parts.PartBlueprint part, Parts.WeaponBlueprintNode node, Vector2 partRectSize)
	{
		foreach (var attachPoint in part.partAttachPoints)
		{
			//Only generate a new button if there is an open attachment slot
			if(attachPoint.attachedPart == null)
			{
				//Generate green attach point
				CallbackTextureButton newAttachPointButton = default;
				newAttachPointButton = CreateCallbackButtonFromAttachmentPoint(attachPoint,() => {SetSelectedAttachPoint(attachPoint, newAttachPointButton, node);}, partRectSize, true, false);

				partVisualizerContainer.AddChild(newAttachPointButton);
				//Set callback to SetSelectedAttachPoint
			}
		}
	}

	//void GenerateAttachPointsUIFromCurrentParts(Parts.WeaponBlueprintNode node)
	//{   
	//    //Clear current Attach points UI
	//    attachPoints.Clear();
	//    //TODO put Attach points in their own UI
	//    foreach (var part in currentParts)
	//    {
	//        GenerateAttachPointsUIFromPart(part);
	//        //Generate buttons that are green attachment points
	//        //they have a callback to be red and set themselves as selected and call SetSelectedAttachPoint on themselves
	//    }
	//}

	void GeneratePartVisualizerUIFromCurrentParts()
	{
		ClearPartsVisualizer();
		ClearCurrentBlueprintDetails();

		Parts.PartStats summationStats = new Parts.PartStats();

		baseNode.IterateNode((node) => 
		{
			//Loads the part visualizer with callbacks to load the part selections
			//cannot pass BPPieceButton to the functor so need to initialize it to an object. 
			CallbackTextureButton BPPieceButton = default;
			BPPieceButton = CreateCallbackButtonFromBlueprint(node.part, () => 
			{
				if(BPPieceButton != selectedPart)
				{
					UpdateCurrentlySelectedPart(BPPieceButton);
					UpdateCurrentlySelectedAttachPoint(null, null);
					ClearPartSelection();
					LoadPartSelection(node.part,node);
				}
			}, partVisualizerScale,true, true, false);

			//With or without parent
			BPPieceButton.RectPosition += (-node.part.baseAttachPoint) * new Vector2(partVisualizerScale.x, partVisualizerScale.y);

			if(node.parent != null)
			{
				BPPieceButton.RectPosition += (node.offset) * new Vector2(partVisualizerScale.x,partVisualizerScale.y);
			}

			BPPieceButton.Modulate = new Color(1,1,1,1);
			partVisualizerContainer.AddChild(BPPieceButton);

			//Place the attachment parts ontop of the actual parts

			GenerateAttachPointsUIFromPart(node.part, node, BPPieceButton.RectSize * BPPieceButton.RectScale / 2.0f);
		});

		//if(currentParts.Count >= 2)
		//{
		//    summationStats = Parts.PartStats.GetCombinationOfStats(currentParts[0].stats, currentParts[1].stats);
		//    for (int i = 2; i < currentParts.Count; i++)
		//    {
		//        summationStats = Parts.PartStats.GetCombinationOfStats(summationStats, currentParts[i].stats);
		//    }
		//}
		//else if(currentParts.Count == 1)
		//{
		//    summationStats = currentParts[0].stats;
		//}

		RichTextLabel bpDetails = new RichTextLabel();
		currentBlueprintDetailContainer.AddChild(bpDetails);
		bpDetails.BbcodeEnabled = true;

		//Only write text if we have parts
		//if(currentParts.Count >= 1)
		//    bpDetails.BbcodeText = summationStats.GenerateStatText(0, false);
		
		bpDetails.RectMinSize = new Vector2(32,50);
		bpDetails.RectClipContent = false;
	}

	public override void _Draw()
	{

		Vector2 center = (partVisualizerContainer as Control).RectGlobalPosition;// + (partVisualizerContainer as Control).RectSize * 0.5f;
		//Vector2 center = (partVisualizerContainer as Control).RectGlobalPosition + (partVisualizerContainer as Control).RectSize * 0.5f;

		//Y axis
		DrawLine(center, center + new Vector2(0,100),new Color("fc0303"),2);  //Pos Y
		DrawLine(center, center + new Vector2(0,-100),new Color("fcdb03"),2);  //Neg Y
		DrawLine(center, center + new Vector2(100,0),new Color("0345fc"),2);  //Pos X
		DrawLine(center, center + new Vector2(-100,0),new Color("fc03ce"),2);  //Neg X
		
		baseNode.IterateNode((node) => 
		{
			//Location that is 0,0 for the part vizualiser

			//Vector from the part base attach node to the center
			Vector2 partBaseAttachNodeToPartCenter = center + (-node.part.baseAttachPoint) * new Vector2(partVisualizerScale.x, partVisualizerScale.y);
			DrawLine(center, partBaseAttachNodeToPartCenter, new Color("00e5ff"), 2);
			
			//Represents the vector from the center of the child node anchor 0.5,0.5 (center of the object) to the child node's base attachment point
			//Vector2 parentAttachNodeToParentCenter = center + -(0.5f * node.part.texture.GetSize() - node.part.partAttachPoints[]) * new Vector2(partVisualizerScale.x,-partVisualizerScale.y);
			//child center to child base attach pt
			//DrawLine(center, centerToAttachPtChild, new Color(0,0,1),4);

			//Center to parent attach pt

			if(node.parent != null)
			{
				//Represents the vector from the parents attachPoint to the center of the child node anchor 0.5,0.5 (center of the object)
				Vector2 attachPtToCenterChild = partBaseAttachNodeToPartCenter + (node.offset) * new Vector2(partVisualizerScale.x,partVisualizerScale.y);
				//attach pt to child center
				DrawLine(partBaseAttachNodeToPartCenter, attachPtToCenterChild, new Color(0.5f,1,0),4);
			}

		});
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
		//Start the current part as a empty handle
		currentParts.Add(allPartsDict[Parts.PartType.Handle][0]);
		baseNode.part = allPartsDict[Parts.PartType.Handle][0];

		//LoadPartSelection(allPartsDict[Parts.PartType.Handle][0]);
		//GeneratePartVisualizerUIFromCurrentParts();
		//LoadBlueprints();

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
	
	
	//Call update which calls _Draw
	public override void _Process(float delta)
	{
		Update();
	}

	int GetMinYSizeFromRichTextLabel(RichTextLabel label)
	{
		//min size is num lines * font size + spacings
		return (1 + label.BbcodeText.Count("\n")) * ((label.Theme.DefaultFont as DynamicFont).Size + (label.Theme.DefaultFont as DynamicFont).ExtraSpacingBottom + (label.Theme.DefaultFont as DynamicFont).ExtraSpacingTop);
	}

	//Loads the list of all possible parts of the passed part blueprint
	public void LoadPartSelection(Parts.PartBlueprint currentBlueprint, Parts.WeaponBlueprintNode currentNode)
	{
		//Load all parts of this type
		foreach (var part in allPartsDict[currentBlueprint.partType])
		{
			//Load part as clickable button with callback to set the current piece of the current blueprint as this piece
			CallbackTextureButton partSelectionButton = CreateCallbackButtonFromBlueprint(part, () => 
			{
				ClearPartSelection();
				//currentParts.Remove(currentBlueprint);
				//currentParts.Add(part);
				//currentParts.Sort(Parts.PartTypeConversion.CompareParts);
				currentNode.part = part;
				//currentNode.offset = 
				GeneratePartVisualizerUIFromCurrentParts();
			}, new Vector2(1,1), false, true, true);
			
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

	//Loads the list of all possible parts of the passed attachment point
	public void LoadPartSelectionAttachPoint(Parts.AttachPoint attachPoint, Parts.WeaponBlueprintNode parentNode)
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
					attachPoint.attachedPart = part;
					//Set the x/y pos of the attach point to the actual node that represents the part
					parentNode.children.Add(new Parts.WeaponBlueprintNode(part, attachPoint.pos, parentNode)); 
					//currentParts.Add(part);
					//currentParts.Sort(Parts.PartTypeConversion.CompareParts);
					GeneratePartVisualizerUIFromCurrentParts();
				}, new Vector2(1,1), false, true, false);

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
}
