<img src="https://github.com/user-attachments/assets/bd08c8bd-f610-449d-b1e2-6a990e562518">


**PixiEditor** is a universal 2D platform that aims to provide you with tools and features for all your 2D needs. Create beautiful sprites for your games, animations, edit images, create logos. All packed in an eye-friendly dark theme.     


[![Release](https://img.shields.io/github/v/release/flabbet/PixiEditor)](https://github.com/flabbet/PixiEditor/releases) 
[![Downloads](https://img.shields.io/github/downloads/PixiEditor/PixiEditor/total)](https://github.com/flabbet/PixiEditor/releases)
[![Discord Server](https://badgen.net/badge/discord/join%20chat/7289DA?icon=discord)](https://discord.gg/qSRMYmq)
[![Subreddit subscribers](https://img.shields.io/reddit/subreddit-subscribers/PixiEditor?label=%20r%2FPixiEditor&logoColor=%23e3002d)](https://reddit.com/r/PixiEditor)
[![Forum](https://img.shields.io/badge/PixiEditor-Forum-red?link=https%3A%2F%2Fforum.pixieditor.net%2F)](https://forum.pixieditor.net/)

### Check out our website [pixieditor.net](https://pixieditor.net) and [PixiEditor Forum](https://forum.pixieditor.net/)

## About PixiEditor

PixiEditor aims to be all-in-one solution for 2D image editing, we aim to achieve this by building a solid foundation with basic functionalities, and exposing complex extension system, that would customize PixiEditor for all your needs.

The project started as a pixel-art editor, but quickly evolved into something much more complex. Version 1.0 was downloaded over 100 000 times on all platforms and received 93% positive rating on Steam.

### Familiar interface

Have you ever used Photoshop or Gimp? Reinventing the wheel is unnecessary, we wanted users to get familiar with the tool quickly and with ease. 

![](https://opencollective-production.s3.us-west-1.amazonaws.com/account-long-description/d2e269a7-8ded-4e0a-a723-c014730dba1c/PixiEditor_6OoxS5PGVD.png)

### Toolsets for any scenario

PixiEditor 2.0 comes by default with multiple toolsets: 
- Pixel art - it contains tool suited for pixel-perfect scenarios
- Painting - Basic painting tools, soft brushes, anti aliased shapes
- Vector - Shapes and paths for creating vectors

All toolsets can be used on one canvas, mix vector with raster. Export to png, jpg, svg, gif, mp4 and more!

![](https://github.com/user-attachments/assets/605c901a-24aa-4c91-9ef9-0fa44878b614)

### Animations

Version 2.0 comes with Timeline and animation capabilities. You can create frame by frame animations or use nodes to animate your custom shaders.
Key frame animations with vectors are planned.

![PixiEditor_YdWFRnYxfb](https://github.com/user-attachments/assets/8fba0c6c-35c8-4ccb-9d69-d6beaff5d97f)

### Nodes

Node render system is what powers such extensive capabilities. All layers, effects, layer structure are nodes or a result of node connections. PixiEditor exposes node graph for every document, so you are free to customize your image however you want and create procedural art/animations!

Here are some examples of what you can do with custom nodes https://pixieditor.net/blog/2024/08/16/devlog7#madeinpixieditor20

## Installation - PixiEditor 2.0

Currently version 2.0 is in open beta, follow this guide to install it https://pixieditor.net/docs/open-beta

## Installation PixiEditor 1.0 - Pixel Art Editor

<a href='//www.microsoft.com/store/apps/9NDDRHS8PBRN?cid=storebadge&ocid=badge'><img src='https://developer.microsoft.com/store/badges/images/English_get-it-from-MS.png' alt='Microsoft Store badge' width="184"/></a>

Get it on Steam now!

[![Get PixiEditor on Steam](https://user-images.githubusercontent.com/121322/228988640-32fe5bd3-9dd0-4f3b-a8f2-f744bd9b50b5.png)](https://store.steampowered.com/app/2218560/PixiEditor__Pixel_Art_Editor?utm_source=GitHub)

**Or**

Follow these instructions to get PixiEditor working on your machine.

1. Download the zipped installer from our [official website](https://pixieditor.net/download)
2. Extract the installer from the archive
3. Launch it
4. Follow the steps in the installer to finish the installation

## Featured content

### PixiEditor 1.0 Trailer

[![Trailer](https://img.youtube.com/vi/UK8HnrAQhCo/0.jpg)](https://www.youtube.com/watch?v=UK8HnrAQhCo)

### Pixel Art Timelapse - "Bog Landscape" | PixiEditor

[![Landscape timelapse](https://img.youtube.com/vi/bzC-wy6HCB8/0.jpg)](https://www.youtube.com/watch?v=bzC-wy6HCB8)

### Gallery

Check out some pixel arts made with PixiEditor [here](https://github.com/PixiEditor/PixiEditor/wiki/Gallery).


## Support

Struggling with something? You can find support in a few places:

* Check out [documentation](https://pixieditor.net/docs)

* Ask on [Discord](https://discord.gg/qSRMYmq)
* Check out [Forum](https://forum.pixieditor.net)
* Open new [Issue](https://github.com/flabbet/PixiEditor/issues)
* [Get help](https://pixieditor.net/help)


## Building from source

### Software Requirements

* .NET 8 SDK
* [wasi-sdk](https://github.com/WebAssembly/wasi-sdk) - PixiEditor uses WASI modules for extensions

### Instructions

1. Clone Repository with nested submodules

`git clone --recurse-submodules -j8 https://github.com/PixiEditor/PixiEditor.git`

or if cloned already, init submodules with

```
cd PixiEditor
```
```
git submodule update --init --recursive
```

2. Download [Wasi-sdk](https://github.com/WebAssembly/wasi-sdk/releases) release for your system
3. Extract downloaded sdk 
4. Set `WASI_SDK_PATH` enviroment variable to extracted directory
5. Run 
```
dotnet workload install wasi-experimental
```

7. Open PixiEditor/src/PixiEditor.sln in Visual Studio or other IDE of your choice

8. Build solution and run PixiEditor.Desktop project

## Contributing 

Start with [Contributing Guide](https://github.com/PixiEditor/PixiEditor/blob/master/CONTRIBUTING.md)

## License

This project is licensed under the LGPLv3 License - see the [LICENSE.md](https://github.com/flabbet/PixiEditor/blob/master/LICENSE) - file for details
