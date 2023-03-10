# TexturePackTool
A very simple texture packer in WPF (Windows only) made for MonoGame.

![image](https://user-images.githubusercontent.com/30244654/224281532-0d211f0b-35f6-422a-af60-bcd4feb99395.png)

Features:
 - add any number of spritesheets with any number of images each, called frames
 - export to create a JSON file describing the frame {x,y,width,height} with the image
   - export without offset (no added borders)
   - export for half-pixel offset (adds a 1px transparent border around everything). Ideal for MonoGame

Notes:
 - When you click New, you specify the JSON location. This is also where the exported image will land
 - Frames added will have paths relative to the JSON path.
