
# Godot 2D RogueLike

(Name TBD) is a 2D side scrolling RogueLike Dungeon Crawler made in Godot where the player will delve into a randomly generated dungeon to gather materials and experience to upgrade their skills and weapons. 

The game engine is Godot using C# scripting and I am doing everything myself besides most of the assets, which are from various licensed sources. I use this as a playground for various ideas of gameplay systems that I would like to implement.

## Table of Contents
### [Level World Generation](#world-generation)
#### [Level World Generation Screenshots](#level-world-generation-screenshots)
### [Weapon Crafting System](#weapon-crafting-system)
#### [Weapon Crafting System UI Iteration Screenshots](weapon-crafting-system-screenshots)
#### [Weapon Crafting System Example Weapons](#example-weapons)
### [Overworld Generation](#overworld-level-generation)
### [Lighting Testing](#initial-lighting-testing)

## World Generation
One major part of this game is the world generation. The first iteration the generator randomly selects if each tile is alive or dead based on a initial chance to create a good starting point. It then uses modified rules from [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) for many iterations to generate somewhat natural looking caves. Specifically  based on this [article](https://gamedevelopment.tutsplus.com/tutorials/generate-random-cave-levels-using-cellular-automata--gamedev-9664). I plan to add later world generation methods as it is something that interests me. 

After the initial world generation the generator finds the largest cave and coordinates to each tile inside that cave to select as the playable area. In the first iteration I used an algorithm that set every single tile its own set and then merge adjacent sets to give me the largest cave and use that as the playable area. This was immensely slow and would take several minutes for a 128x128 map which is ridiculous. A flood fill would not work because I have an N number of caves and do not know their starting positions or the minimum size of these caves. I could try to prematurely optimize and generate a sparse grid every 5 pixels or so as the starting location for the flood fill and then merge adjacent sets which would potentially ignore the smaller caves but I would prefer a complete algorithm. In the second iteration I used a [Connected Component Labeling Algorithm](https://en.wikipedia.org/wiki/Connected-component_labeling), which is supposed to be O(NxM), was significantly faster, but also more complicated. Currently a 128x128 level takes around 1-2 seconds to run the entire CCL algorithm, [Implemented here](https://github.com/roushk/Godot-2D-RogueLike/blob/main/2DGodotRogueLike/Scripts/MapGeneration/CCLGenerator.cs), on and I plan on optimizing this at a later date but currently 1-2 seconds is fast enough considering I am not planning to have levels much larger than 128x128 considering the size of the tiles. 

Next I am looking to figure out the best way to place the level starting location and levelending location in addition to figuring out some way to differentiate rooms vs hallways in reguards to spawning enemies and items in the world. Currently my method to figure out larger rooms is to figure out the distance from each ground tile to the wall. I am doing this somewhat effectively by using the Midpoint Circle alg to pregen a series of growing radii to then run simple collision to determin the distance from the edge of a room any tile is and save that data.

My current idea for sorting the rooms spatially is to first run a flood fill to generate a directed graph and then use that with Tarjan’s Algorithm to find choke points. Then after finding the choke points I can run flood fills to create rooms from the areas between each choke point and then run A* on the entire map to figure out the farthest rooms and use those as the starting and ending rooms for the level. My goal is to have the directed graph generate a fairly predictable growth that looks something like tree roots. Currently the directed graph its generated but I need to change the method for queueing up tiles to check in order to generate something closer to what I want as it looping into and out of rooms. 

### Level World Generation Screenshots

## Map with no overlay
![DirGraph_CCL_NoOverlay](https://user-images.githubusercontent.com/34784335/132656123-f0d485be-c162-4d2d-b87a-790598cd53dc.PNG)
## Map with Adjacency Overlay
![DirGraph_AdjacencyOverlay](https://user-images.githubusercontent.com/34784335/132656147-d03d9d7e-fe2a-49a1-af8c-0c8636dc6432.PNG)
## Map with Directed Graph overlay (Arrows point to parent, root is red dot)
![DirGraph_DirectedGraphOverlay](https://user-images.githubusercontent.com/34784335/132656293-6ae349a6-2dd4-46ac-adf9-33b9f2b1cb80.PNG)
## Map with Directed Graph overlay with example unwanted path (Arrows point to parent, root is red dot)
![DirGraph_DirectedGraphOverlay_ExamplePt_FIFO](https://user-images.githubusercontent.com/34784335/132656285-f2e8a477-d6ae-445a-9619-25498a52c163.PNG)

#### Flood Fill Example 1
![FloodFill1](https://user-images.githubusercontent.com/34784335/131393394-c0262dbf-d44d-4f1d-8cc0-d065e0f0b34d.jpg)

### CCL Example 1 128x128 @ 40% initially alive change for Game of Life
![WorkingCCL_128x128](https://user-images.githubusercontent.com/34784335/131573714-36944d2f-0a0a-4880-8ae2-7b8542605c59.PNG)
### CCL Example 2 128x128 @ 45% initially alive change for Game of Life
![WorkingCCL_128x128_45initial](https://user-images.githubusercontent.com/34784335/131573718-b81fb0cc-7e17-4cc5-b8e5-9f4ae4649c72.PNG)
### CCL Example 3 128x128 @ 46% initially alive change for Game of Life
![WorkingCCL_128x128_46initial](https://user-images.githubusercontent.com/34784335/131573719-cbd5f744-6692-4069-8f04-23df30ef42f3.PNG)
### CCL Example 4 256x256 @ 40% initially alive change for Game of Life
![WorkingCCL_256x256_40initial](https://user-images.githubusercontent.com/34784335/131573720-e31051e6-f910-4447-b002-4a5e1dc73ace.PNG)
## CCL Example 5 200x100 @ 44% initial with only largest cave visible
![CompleteCCLWithLargestCave](https://user-images.githubusercontent.com/34784335/131959077-069cbaf5-dd34-4278-af4c-0223fb578c5f.PNG)
## Adjacency overlay of 200x100 
![Adjacency](https://user-images.githubusercontent.com/34784335/131959204-247caa48-6ea0-43b6-8b9c-65408322e3e1.PNG)

#### Map Generation UI
![MapGeneratorUI_Iter2](https://user-images.githubusercontent.com/34784335/131393486-b1128c13-35c0-4cd9-b44d-f899daeb4314.PNG)

## Weapon Crafting System
Another major portion of the game is the weapon creation. The goal of the weapon creation is the player is able to create a weapon that suits not only their playstyle but with combinations of parts the weapon would be effective in combat. I decided on a part and material system where the materials will affect how statistically strong and effective the weapon would be, such as amount of damage on hit or applying a fire debug on enemies that are hit. The parts of the weapon would change how it swings, the swing speed, and all of the other properties of the weapon. 

In the first iteration of the Weapon Crafting System I decided that weapons would be based upon blueprints. For example I had a Rapier blade that required a small handle, rapier blade, and rapier handguard. These would affect the swing speed, damage, and special ability of the weapon respectively. After testing the weapon creation I found it very limiting and not allowing the player to create a new weapon but forcing them to pick an archetype of a weapon, such as big sword, rapier, dagger, and only change slight parts. 

In the second iteration of the Weapon Crafting System I completely changed not only the internal data structure of the weapons, and streamlined the entire art asset loading pipeline, but also decided to adopt a more freeform creation process. The process starts with the player choosing the length of handle that they want. This affects the reach of the weapon along with the swing speed and a damage modifier to the entire weapon. The player then has nodes along the handle, currently just the top and bottom, and they can click on a node to bring up a UI to select a new part to add such as a pommel, blade, pickaxe head, etc. These parts will add to and modify the stats of the weapon. I found this system significantly more fun to create with. 

The change in the core mechanics of the builder I decided to completely refactor the underlying system. The system before used a combination of various Maps and data structures to track the current blueprint and which parts were selected and the materials for those parts but the new system was more agnostic. It is simply a tree where the root node is the handle. This new system was much easier to design but still difficult to implement due to the UI callbacks needed to correctly change and add to the weapons. I am currently facing issues positioning the UI for the parts themselves on the weapon building screen but the underlying tree and how it's updated and changes with the UI works as intended. Next I need to fix the positioning problems and then add the material selection.

The best part of the system is that the entire thing is driven by a single json file now instead of multiple. I have a single file that registers the icon and statistics of the part along with the node connection points and the type of part that can be attached to it such as a blade or a pommel or guard. This more abstract design allows for very different weapons to be created. 

Currently I am fixing some UI placement issues with the Weapon Crafting System as I refactor the underlying system to be more straightforward and follow best practices and become easier to debug and iterate upon. 

### Weapon Crafting System Screenshots

#### Weapon Builder UI Iteration 1
![ModularWeaponsUI_Iter1_1](https://user-images.githubusercontent.com/34784335/131393584-5c975c7c-b9d7-4d29-b02e-e90df36264c4.PNG)

#### Weapon Builder UI Iteration 2
![ModularWeaponsUI_Iter2_1](https://user-images.githubusercontent.com/34784335/131393626-c5073056-ac59-4a28-a55f-7b668b9f2b4e.PNG)

#### Weapon Builder UI Iteration 3
![ModularWeaponsUI_Iter3_2](https://user-images.githubusercontent.com/34784335/131393647-b27c9a21-85fc-461c-a5ee-fc0e07f8a9ec.PNG)
![ModularWeaponsUI_Iter3_1](https://user-images.githubusercontent.com/34784335/131393651-8f6c687b-e1ec-43d4-977d-3137c33c42e0.PNG)

#### Weapon Builder UI Iteration 4
![ModularWeaponsUI_Iter4_2](https://user-images.githubusercontent.com/34784335/131393667-1e830d5c-8530-4add-b8b1-2f0042d3d5e3.PNG)
![ModularWeaponsUI_Iter4_1](https://user-images.githubusercontent.com/34784335/131393668-7ce214cc-6d5c-4fa8-9563-6e1a55a4ab8d.PNG)

### Weapon Builder Iteration 2 with UI Iteration 5 - Post internal system refactor 

#### The UI with a graph of the X/Y that I use for debugging the UI placement
![ModularWeaponsUI_Iter5_PartiallyVisibleToSeeCoords](https://user-images.githubusercontent.com/34784335/131394292-a3f96202-6e21-47d5-af4f-53d812045781.PNG)

#### Selecting a node brings up the part selection
![ModularWeaponsUI_Iter5_SelectedNode](https://user-images.githubusercontent.com/34784335/131394293-36b0ef06-c6af-4d28-8be1-fa67f56421aa.PNG)

### Example Weapons

#### Example Weapon: Basic Sword
![ModularWeaponsUI_Iter5_CreatedSword](https://user-images.githubusercontent.com/34784335/131394299-ff7ac21a-d182-4741-8f16-8e01fdcf9db2.PNG)
#### Example Weapon: Warhammer
![ModularWeaponsUI_Iter5_CreatedWarhammer](https://user-images.githubusercontent.com/34784335/131394301-6cfef866-d7f6-4847-8c72-e0437b562ccb.PNG)
#### Example Weapon: Large Bladed thing?
![ModularWeaponsUI_Iter5_CreatedWeirdLongBladedThing](https://user-images.githubusercontent.com/34784335/131394287-b1b2741d-0fb8-4e3a-95d6-3db4663651fa.PNG)

## Overworld Level Generation
The other algorithm that I am going to implement is [Wave Function Collapse](https://github.com/mxgmn/WaveFunctionCollapse), specifically the Simple Tiled Model. This would be used to generate an Overworld for the player to traverse with cities, lakes, etc. The basic WFC algorithm is that every single tile exists in a state of being all tiles, similar to the Wave Function in Quantum Physics which this algorithm is named after. The goal of the algorithm is to observe, select a specific tile, the tiles with the lowest entropy, chance to change or in this case lowest amount of tiles in the possibility set, and then propagate those changes throughout the map to create a stable state where the entire map is observed. Once this is finished a map is generated. There are rules for the tiles, such as water is only adjacent to the coast etc, and these rules determine the possible set of tiles that any position can be observed as. There are some problems with this algorithm, specifically with observing on larger maps and how depending on if the algorithm is implemented recursively, as would be most natural, may cause stack issues when destroying a branch of possibility with the tile choice. 

There is a paper called [Automatic Generation of Game Levels Based on Controllable Wave Function Collapse Algorithm](https://www.researchgate.net/publication/348204502_Automatic_Generation_of_Game_Levels_Based_on_Controllable_Wave_Function_Collapse_Algorithm) that delves deeper into using the WFC Simple Tiled Model for game level generation. They introduce the idea of constraints and layering to the model along with the idea of symmetry of tiles and the ways to transform the tile images to generate more effective tiles from a single art asset. What I think was most interesting is their usage of a multi staged level generation system where they would constrain and generate the ground level of a world and then on top of it add other levels such as the road, chests, keys, and enemies in different stages and used the stages beforehand as additional constraints for the later stages. 

My goal is to have a multi staged WFC Simple Tiled Model level generator that builds upon the stages before and leverage this with the AutoTile system in Godot to automatically tile the entire map. 

Other great sources for WFC:
[Wave Function Collapse](https://github.com/mxgmn/WaveFunctionCollapse)
[Boris the Brave's Explanation of WFC](https://www.boristhebrave.com/2020/04/13/wave-function-collapse-explained/)
[Caves of Qud using WFC as a stage of Level Gen](https://www.gdcvault.com/play/1026263/Math-for-Game-Developers-Tile)
[Oskar Stalberg WFC in Bad North](https://www.youtube.com/watch?v=0bcZb-SsnrA)

### Generated World Goal 
![example_overworld](https://user-images.githubusercontent.com/34784335/131397301-5ce03d8a-ce7e-41da-8a22-a93bfbfad41b.PNG)

### Initial Lighting Testing

For this game I wanted to have a unique art style but sadly I am not a skilled artist I decided to leverage my knowledge of 3D graphics and PBR, IBL, etc to make the game look pretty without beautiful assets. While researching 2D platformer/adventure/metroidvania games for a different project I came across this very beautiful game [Seige and Sandfox](https://siegeandsandfox.com/). I appreciated their lighting and art style, they managed to make the game feel like an old school 8bit game while having very pretty lighting. Watching their talk [Beyond Authentic Pixel Art in Unreal Engine](https://www.youtube.com/watch?v=TLbXNYK4928) online and seeing the upgrade and depth that normal maps added to their world inspired me to try it myself. I decided to add normal mapping with lighting to the 2D sprites and see what that type of 2D/3D depth effect would look like. 

It took me a day or two but after digging through Godot's documentation found out that they have a 2D/3D lighting pipeline and after plugging in a normal map I found online and one I created for some ingots I got decently good looking lighting and plan to create a larger test scene with walls and props to see if I want to continue down this path. 

#### Initial Lighting example
https://user-images.githubusercontent.com/34784335/132577652-1004471e-d819-494d-b8c5-1cf4db1ea357.mp4
