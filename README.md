# TexturePackTool
A simple texture packer that generates a texture atlas and associated JSON file. It can also be done by command line.
![image](https://github.com/NinthDesertDude/TexturePackTool/assets/30244654/69172818-6efc-4223-a759-edb5c2558b91)

The output:
```json
{"sheets":[{"export-url":"untitled_spritesheet_1","frames":[{"name":"block1","path":"block1.png","x":0,"y":0,"w":1922,"h":169},{"name":"block2","path":"block2.png","x":1597,"y":168,"w":40,"h":131},{"name":"block3","path":"block3.png","x":819,"y":168,"w":760,"h":116},{"name":"block4","path":"block4.png","x":1578,"y":168,"w":20,"h":699},{"name":"block5","path":"block5.png","x":819,"y":283,"w":695,"h":220},{"name":"block6","path":"block6.png","x":0,"y":168,"w":820,"h":1030},{"name":"block7","path":"block7.png","x":1686,"y":168,"w":3,"h":7},{"name":"block8","path":"block8.png","x":1636,"y":168,"w":35,"h":4},{"name":"block9","path":"block9.png","x":1688,"y":168,"w":5,"h":4},{"name":"block10","path":"block10.png","x":1670,"y":168,"w":17,"h":33}],"name":"Blocks"},{"export-url":"untitled_spritesheet_2","frames":[],"name":"Untitled_Spritesheet_2"}]}
```

Texture splitting supports black borders as well as half-pixel offset:  
![image](https://github.com/NinthDesertDude/TexturePackTool/assets/30244654/dad2e800-5c98-44d2-bd0a-a359b0f6f1fb)

In addition to exporting packed images, it supports splitting any number of spritesheets with a tile grid into separate files under multiple folders:  
![image](https://github.com/NinthDesertDude/TexturePackTool/assets/30244654/59a23557-94af-41db-9afd-a2231c11f3de)

Why TPT?
I didn't see much readily available freeware for this. I have a powerful texture asset flow based on this:  
- Artists make individual tiles or objects  
- I create a TPT project for all the individual tiles, grouped into spritesheets based on common dimensions such that a grid fits each one  
- I can use programs like Tiled with these exported spritesheets since they each fit a grid, no wasted tile IDs or images cut-up across tiles  
- I create a TPT project for all the exported spritesheets to export for GPU. Adjust to your heart's content without worrying about breaking code  
- On pre-build, I run a powershell script that reads the JSON of both TPT projects to produce strongly-typed code available at compile time for frames  
  - I use this to avoid easy breakages from string names, and make animations easily  
- Since this has command line support, you could rebuild both TPT projects on pre-build so you always pick up changes to any tile art  
- When drawing a tile from Tiled, add the x,y offset for the src rect when those tiles are included in the 2nd TPT project. The offset is just the x,y of that frame

I also search a virtual directory using folder symmetry to find mod overrides of built-in assets. I can rebuild my texture atlases by command line this way at runtime, keeping speed and still break nothing :)
