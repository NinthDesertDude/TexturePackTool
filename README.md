# TexturePackTool
A simple texture packer that generates a texture atlas and associated JSON file. It can also be done by command line.
![image](https://github.com/user-attachments/assets/ded29617-b6df-484e-96e1-4789ee378276)

The output:
```json
{"sheets":[{"export-url":"untitled_spritesheet_1","frames":[{"name":"block1","path":"block1.png","x":0,"y":0,"w":1922,"h":169},{"name":"block2","path":"block2.png","x":1597,"y":168,"w":40,"h":131},{"name":"block3","path":"block3.png","x":819,"y":168,"w":760,"h":116},{"name":"block4","path":"block4.png","x":1578,"y":168,"w":20,"h":699},{"name":"block5","path":"block5.png","x":819,"y":283,"w":695,"h":220},{"name":"block6","path":"block6.png","x":0,"y":168,"w":820,"h":1030},{"name":"block7","path":"block7.png","x":1686,"y":168,"w":3,"h":7},{"name":"block8","path":"block8.png","x":1636,"y":168,"w":35,"h":4},{"name":"block9","path":"block9.png","x":1688,"y":168,"w":5,"h":4},{"name":"block10","path":"block10.png","x":1670,"y":168,"w":17,"h":33}],"name":"Blocks"},{"export-url":"untitled_spritesheet_2","frames":[],"name":"Untitled_Spritesheet_2"}]}
```

Texture packing lets you export packed images with options like a transparent or black border (showing black):
![image](https://github.com/NinthDesertDude/TexturePackTool/assets/30244654/69172818-6efc-4223-a759-edb5c2558b91)

You can separate spritesheet(s) by grid or regions of pixels into files for each tile or island:
![image](https://github.com/user-attachments/assets/18d04877-a014-4bde-9add-8cd5fcf9285c)

Why TPT?
I didn't see much readily available freeware for this, especially by command line. I use it in my game asset pipelines with a script to generate code at compile-time that makes it easy to reference sprites without updating the code when sprites are renamed or the sprite atlas changes. The atlas is useful to GPU.

You can use command-line to rebuild texture atlases at runtime in games/programs, which is useful with modding environments e.g. folder symmetry + texture replacements.
