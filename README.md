# TexturePackTool
A very simple texture packer in WPF (Windows only) made for MonoGame. It lets you add any number of spritesheets. For each one, it lets you import any number of images as frames that will be automatically arranged for a reasonably tight fit in the spritesheet, handling images with different dimensions. It also exports a JSON file using the frame names you entered, and their {x,y,width,height} positions. You can export with a transparent border around the images (for engines that use halfpixel offsets like MonoGame), or not.

![image](https://user-images.githubusercontent.com/30244654/224281532-0d211f0b-35f6-422a-af60-bcd4feb99395.png)
