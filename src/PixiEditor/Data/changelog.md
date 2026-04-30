# 2.1.0.23

Date: `29.04.2026`

- Added new "Foam" brush,
- Added a lot of new sample files
- Improved Basic and Claws brushes opacity handling
- Bunch of crash fixes

# 2.1.0.20

Date: `18.04.2026`

## Brush Engine

![image|426x459](https://forum.pixieditor.net/uploads/default/original/1X/99c6ffa98ee02800272cbe3d3b8a6998f6d63a15.png)

Likely, the most awaited of features, you can now create your own Brushes with PixiEditor's Node Graph!

Use the new Brush Picker within Pen Tool to select, duplicate and edit brushes.

Along the Brush Engine, full drawing tablet support has been added. For Windows, both Ink and WinTab APIs are supported.

Check out the [documentation](https://pixieditor.net/docs/usage/brushes/getting-started) to learn how to create your own brushes.

**Brushes behave differently within Pixel Art and Painting toolset!**

## Extension Browser Beta

![image|690x405](https://pixieditor.net/_astro/wsp.Cx8QtmYt_ZOD8MR.webp)

You can now browse and install extensions! [Learn more about the beta](https://forum.pixieditor.net/t/extension-browser-beta/567)

In short:
- This is a Phase 1, so the amount of extensions is very limited and only developed by Pixi Labs,
- Phase 1 is focused on preparing the system for community extensions and funding the further phases

## Precise paint engine

PixiEditor now uses precise paint engine. Previously all brush stamps were rounded to pixels, starting from version 2.1, you can paint with sub-pixel precision. Snapped pixels are still an option per-brush.

## Stabilizers

![image|189x46](https://forum.pixieditor.net/uploads/default/original/1X/5b1504b3664667695667ea672891808ac281f86c.png)

We've added 2 stabilizers: Distance based and Time based. They are available within pen tool settings in the viewport.

**Time based**: Stabilization is calculated based on time passed between previous position and new position.

**Distance based**: You need to move X pixels before brush follows your pointer. It's like dragging something with a rope, you need to move far enough for the rope to become tight, before it draggs the thing on the other side.

## Smart Layers

![image|365x139](https://forum.pixieditor.net/uploads/default/original/1X/19300fb77f3efbb47675eb08620b5d9908ac8555.png)

PixiEditor now supports smart layers. You can use other .pixi files within your document, embed .svg files directly as layers and more.

PixiEditor's advanced Node Graph allows to expose values to parent document, if they are embedded as a nested document (smart layer). Additionally, with new Blackboard feature, you can define inputs to your smart layers.

So for example you can create a nice text animation template and create "Text" blackboard input.
Then after embedding this animation within other file, you can type your text and it will be passed to the nested document.

![image|340x500](https://forum.pixieditor.net/uploads/default/original/1X/5347e50b347df7ab5699948c3147e3f8c4a8f774.png)

This way you can reuse one file multiple times and compose your graphics/animations much more easily.

## Blackboard

Blackboard is our solution for multiple, important things:

![image|286x162](https://forum.pixieditor.net/uploads/default/original/1X/2b2281f08e25c2d844ef4cf1176c40d382411002.png)

- It serves as a way to define and reuse constants within one Node Graph
- It defines Brush settings, that are exposed in Pen tool settings when brush is picked
- It allows for defining inputs for graphs. Inputs are a gateway to pass data between parent document and embedded one. If you prefer programming comparision, you can think of a nested graph as a function and inputs as function parameters.

## Improved animation timeline

This is something many of you requested. We’ve overhauled the timeline and workflow of the animation system. This is the very first [community proposal](https://forum.pixieditor.net/t/timeline-improvements/540) coming to life.

## New Renderer

As a response to a few important issues, We rewrote a lot of core rendering components. There are plenty of new optimizations and characteristics. A few issues, that the new renderer is trying to solve:

- Bad performance when running some animations
- Better responsiveness of the app

## New Nodes

- Gradient Node: Create gradients procedurally
- Pattern Node: Draw vector stroke out of input image.
- Viewport Info: Information about active viewport
- Blackboard Variable Value: Access blackboard variables
- Stroke Info: Info about current Brush stroke, only works within Brushes
- Brush Output: A node for defining brushes, see Brush Engine section
- Pixel Perfect Ellipse: New shape, that creates pixel-perfect ellipses
- Switch: Choose a value based on a condition
- Equals: Are two items equal
- Expose Value: Exposes arbitrary value from the graph as a output, when nested into other document.
- Editor Info: Info about current editor state, currently only Primary and Secondary color
- Nested Document: Smart Layer node. Allows for picking any document file.
- Pointer Info: Info about current pointer, such as pressure, position on canvas and more.
- Keyboard Info: Information about keyboard state, currently bools about modifier keys.
- Array: Create an array out of items
- Array Element: Get an element from array by index
- Array Length: Get length of an array

## New Sockets

You may notice new colorful inputs in the graph, they mean that any type can be plugged into them.

![image|690x405](https://forum.pixieditor.net/uploads/default/original/1X/8089ae27717d306987e78e23b5bc4685619b9b7f.png)

## Customizable Toolsets and Brush Tools

You can now create your own toolsets and tools based on Brushes.

Check out [the documentation](https://pixieditor.net/docs/usage/brushes/brush-tools) to learn how.

## Other enhancements

- Importing images now embed them as smart layers by default.
- Added Mushy advisor to various parts of the app, to help you understand how to use them.
- Improved string editor. Now it supports syntax highlighting for shaders, search and replace and more.
- Improved stability of OpenGL render api
- Clipboard should now work much better and more stable.
- Tons of other smaller improvements, features and bug fixes, that you can check out in the [full changelog](https://forum.pixieditor.net/t/pixieditor-2-1-0-20/578)