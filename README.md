# Godot 2D RogueLike

(Name TBD) is a 2D side scrolling RogueLike Dungeon Crawler made in Godot where the player will delve into a randomly generated dungeon to gather materials and experience to upgrade their skills and weapons. 

The game engine is Godot using C# scriptingand I am doing everything myself besides most of the assets, which are from various licensed sources. I use this as a playground for various ideas of gameplay systems that I would like to implement.


## Table of Contents
### [World Generation](#world-generation)
#### [World Generation Screenshots](#world-generation-screenshots)
### [Weapon Crafting System](#weapon-crafting-system)
#### [Weapon Crafting System UI Iteration Screenshots](weapon-crafting-system-screenshots)
#### [Weapon Crafting System Example Weapons](#example-weapons)
 
## World Generation
One major part of this game is the world generation. For the first iteration the generator uses perlin noise to populate an array to get a good starting point. It then uses modified rules from [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) for many iterations to generate somewhat natural looking caves. In the first iteration I used a simple flood fill to give me the largest cave and use that as the playable area but in the second iteration I used a [Connected Component Labeling Algorithm](https://en.wikipedia.org/wiki/Connected-component_labeling), which is supposed to be around 4x faster but also more complicated. It is currently being refactored to fix a bug for the parent relationship of the partial neighborhood. 

### World Generation Screenshots

#### Flood Fill Example 1
![FloodFill1](https://user-images.githubusercontent.com/34784335/131393394-c0262dbf-d44d-4f1d-8cc0-d065e0f0b34d.jpg)

#### Flood Fill Example 2
![FloodFill2](https://user-images.githubusercontent.com/34784335/131393442-a87ecde1-055c-4864-8990-cda83841e556.jpg)

#### Flood Fill Example 3
![FloodFill3](https://user-images.githubusercontent.com/34784335/131393445-9022c691-6c87-4ea4-9f47-4aca4d5b6ab4.JPG)

#### Map Generation UI
![MapGeneratorUI_Iter2](https://user-images.githubusercontent.com/34784335/131393486-b1128c13-35c0-4cd9-b44d-f899daeb4314.PNG)

## Weapon Crafting System
Another major portion of the game is the weapon creation. The goal of the weapon creation is the player is able to create a weapon that suits not only their playstyle but with combinations of parts the weapon would be effective in combat. I decided on a part and material system where the materials will affect the how statistically strong and effective the weapon would be, such as amount of damage on hit or applying a fire debug on enemies that are hit. The parts of the weapon would change how it swings, the swing speed, and all of the other properties of the weapon. 

In the first iteration of the Weapon Crafting System I decided that weapons would be based upon blueprints. For example I had a Rapier blade that required a small handle, rapier blade, and rapier handguard. These would affect the swing speed, damage, and special ability of the weapon respective. After testing the weapon creation I found it very limiting and not allowing the player to create a new weapon but forced them to pick an archetype of a weapon, such as big sword, rapier, dagger, and only change slight parts. 

In the second iteration of the Weapon Crafting System I completely changed not only the internal data structure of the weapons, and streamlined the entire art asset loading pipeline, but also decided to adopt a more freeform creation process. The process starts with the player choosing the length of handle that they want. This affects the reach of the weapon along with the swing speed and a damage modifier to the entire weapon. The player then has nodes along the handle, currently just the top and botton, and they can click on a node to bring up a UI to select a new part to add such as a pommel, blade, pickaxe head, etc. These parts will add to and modify the stats of the weapon. I found this system significantly more fun to create with. 

The change in the core mechanics of the builder I decided to completly refactor the underlying system. The system before used a combination of various Maps and data structures to track the current blueprint and which parts were selected and the materials for those parts but the new system was more agnostic. It is simply a tree where the root node is the handle. This new system was much easier to design but still difficult to implement due to the UI callbacks needed to correctly change and add to the weapons. I am currently facing issues positioning the UI for the parts themselves on the weapon building screen but the underlying tree and how its updated and changes with the UI works as intended. Next I need to fix the positoning problems and then add the material selection.

The best part of the system its the entire thing is driven by a single json file now instead of multiple. I have a single file that registers the icon and statistics of the part along with the node connections points and the type of part that can be attached to it such as a blade or a pommel or guard. This more abstract design allows for very different weapons to be created. 

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

