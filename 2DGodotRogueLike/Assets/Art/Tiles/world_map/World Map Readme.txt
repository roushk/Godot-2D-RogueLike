Hello, fellow game developer!

This is my biggest and most comprehensive tileset yet, so I thought I'd give a few tips on how it works.
First off, you can find all the PNG files you need to build your world map in the "pieces" folder. But you probably figured that out already!

In pieces/terrain, you'll find a bunch of images with the suffixes _f1, _f2, and _f3. These indicate images that are part of an animation. If you want to use animation, display the frames in the following order:
f1 -> f2 -> f3 -> f2, then loop.

In pieces/mountains, you can find premade mountains as well as pieces you should be able to use to create your own mountains.

In pieces/objects, you can find towns, bridges, roads, the castle, trees, and caves.

All of the PNGs are composed of 16x16 tiles, and are designed to fit on a 16x16 tile grid.
Refer to world_map_example.psd or world_map_wallpaper.png to see how to put everything together.
The layer order is:
- Objects (top)
- Mountains
- Terrain (bottom)

If you have any questions, get in touch with me!
-Will
Patreon: patreon.com/untiedgames
Twitter: @untiedgames
Facebook: facebook.com/untiedgames