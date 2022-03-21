# A potential new architecture for PixiEditor's backend.

Decouples the state of a document from the UI.

## Roadmap

- ChunkyImage
    - [x] Basic commited and preview chunk support
    - [x] Affected chunk infrastructure
    - [x] Size constraining
    - [x] Cloning
    - [ ] Periodic cleaning of empty chunks
    - [x] ChunkPool multithreading support
    - [x] Dispose that returns borrowed chunks
    - [ ] Finalizers that return borrowed chunks
    - [ ] GetLatestChunk resolution parameter
        - [ ] Support for chunks of different sizes?
        - [ ] Disposable ChunkView class?
    - [x] CommitedChunkStorage (used to store chunks for undo)
        - [ ] Write chunks to the hard drive?
    - [ ] Linear color space for blending
    - [ ] Tests for everything related to the operation queueing
    - Operations
        - [ ] Support for paints with different blending (replace vs. overlay)
        - [ ] Image (basic done, needs rotation, scale, skew, perspective, etc.)
        - [ ] Rectangle (basic done, needs rotation support)
        - [ ] Ellipse
        - [ ] Line
        - [ ] Fill
        - [ ] Draw pixels
        - [ ] Translate everything
        - [x] Clear operation
        - [x] Clear region operation
        - [x] Resize canvas operation
        - [ ] Resize image operation?
        - [x] Raster clip operation
        - Filters
            - [ ] Hue/Saturation/Value
            - [ ] Brightness/Contrast
            - [ ] Curves
            - [ ] Gradient mapping
- ChangeableDocument/Renderer
    - [x] Basic Action->Change->ChangeInfo pipeline
    - Undo handling
        - [x] UpdateableChange class for changes requiring preview
        - [ ] Handling for changes that don't change anything
        - [x] Dispose changes
        - [ ] Dispose the entire document
        - [x] Basic undo stack infrastructure
        - [ ] Ignored changes (changes that don't get recorded in undo)
        - [ ] Manually ignore specific changes
        - [ ] Manual squashing
        - [ ] Auto-squashing
        - [ ] Limit undo stack size (discard old changes)
    - [x] Basic Collect Actions -> Apply actions -> Render changes pipeline
    - Rendering
        - [x] Basic layer stack rendering
        - [x] Opacity/Visibility support
        - [x] Recursive rendering of folders
        - [ ] Blending modes support
        - [ ] Clip to layer below support
        - [ ] Layer mask support
        - [ ] Low-res rendering
        - [ ] Don't render chunks outside viewport
        - [ ] Vector layers (primarily reference layer) rendering
        - [ ] Support for rendering a subset of the structure (for merging selected layers, referring to selected layer, etc.)
        - [ ] Caching for folders
        - [ ] Caching for everything below current layer
        - [ ] Caching for sequences of layers with a normal blending mode
        - Rendering images for changes (tools requiring final image, merge layers, etc.)
            - [ ] Action packeting with a pre-rendering step for each packet
            - or
            - [ ] StructureRenderer as a part of Document
        - [ ] Rendering of layer previews
        - [ ] Rendering of canvas previews
        - [ ] Rendering of the navigator image
    - Changes
        - [x] Layer structure changes
        - [ ] Merge layers
        - [ ] Vector layers manipulation (or at least reference layer)
        - [ ] Resize canvas (basic done, anchors left)
        - [ ] Resize image
        - [ ] Rectangle (basic done, rotation left)
        - [ ] Ellipse
        - [ ] Line
        - [ ] Pen
        - [ ] Pixel-perfect pen
        - [ ] Eraser (same as pen?)
        - [ ] Fill
        - [ ] Brightness
        - [x] Basic selection changes
        - [ ] Selection modes
        - [ ] Circular selection
        - [ ] Magic wand
        - [ ] Lasso
        - [ ] Move/Rotate selection
        - [ ] Transform selection
        - [x] Clip to selection
        - [ ] Clip to itself (preserve transparency)
        - [ ] Layer mask manipulation
- ViewModel
    - [ ] Action filtering
    - [ ] Viewport movement as an action
- Not sure
    - [ ] Pipette tool

## Included cs projects (all wip, most features not implemented yet):

- ### ChunkyImageLib

ChunkyImage is an image consisting of chunks. The goal is to make a simple drawing interface that would let you use ChunkyImages like regular bitmaps without caring about chunks. Features of ChunkyImage include a build-in replacement for the concept of a preview layer, lazy drawing for fast real-time preview, and a downscaled version of the same image for the same purpose.

ChunkyImage replaces previews layers by letting you undo latest drawing commands. It's interface has two functions, `CommitChanges` and `CancelChanges`. `CancelChanges` lets you undo all changes made since the last call to `CommitChanges`. `CommitChanges` sets all changes in stone meaning you can no longer undo them.

Whenever you draw something on a ChunkyImage the image doesn't get updated right away. Instead, the drawing command gets stored internally. The stored commands are only executed when someone tries to access the state of the image. Importantly, if someone tries to access a single chunk only that chunk will be redrawn to the final state, saving on computation. All stored commands are applied to all chunks when `CommitChanges` is called.

ChunkyImage lets you request a downscaled version of each chunk. Oftentimes (if the viewport is zoomed out) you don't need to compute the full-resolution version of the chunk just for the preview. Since the drawing is done lazily, the full-resolution version won't be computed if no one asks for it (until the changes are commited).

- ### ChangeableDocument

ChangeableDocument is a system that keeps track of the full state of a single document. This includes the layer structure, all layer images, the undo/redo histories, etc. ChangeableDocument accepts user actions and changes the document state according to them. The state is publicly accessible for reading, but it can only be changed with actions.

The implementation of ChangeableDocument uses these concepts:

- Action: A piece of data with info about something that's been done, e.g. "Delete layer with some GUID"; "Undo"; "Redo".
- Changeable: A part of the document state. The document itself is a Changeable, any Layer is also a changeable. All a Changeable does is stores the current state of itself.
- Change: A class that lets you mutate a Changeable in some way. A change has `Apply` and `Revert` functions. For example, when deleting a layer a new Change is created. It first gets initialized, then applied. If Undo is called, it gets reverted. On initilization the current state of the layer is saved in the change. On applying the change the layer gets deleted from the document state. On reverting the change the layer gets recreated using the previously saved data.
- ChangeInfo: A piece of data describing the changes made to the state by a Change. It is returned by the `Apply` and `Revert` functions of the Change class.

ChangeableDocument uses ChunkyImages to store layer bitmaps and to draw on them.

Generally, ChangeableDocument will be used in PixiEditor by pumping all user actions into it, getting ChangeInfos back and updating the UI based on them.

- ### StructureRenderer

The main purpose of StructureRenderer is rendering the final visible image from all the layer images. It has access to ChangeableDocument's state and also receives all of it's ChangeInfos. StructureRenderer updates the final visible image when it encounters one or more ChangeInfos that describe some visible change (drawing, canvas size change, etc.). StructureRenderer can use low-resolution version of chunks from ChunkyImages to speed up rendering, but only while a tool is in use. Once you've stopped using the tool it always renders the final full-res image. StructureRenderer also renders layer previews.

StructureRenderer emits it's own ChangeInfos to notify the UI about the changes to the final image. They mainly include dirty rectangles (just the coordinates, not the data) and requests to recreate WriteableBitmaps when canvas size changes.

During the implementation process StructureRenderer will most likely become a part of ChangeableDocument, with the final rendered image and layer previews becoming parts of the document state.

- ### PixiEditorPrototype

A mockup UI with viewmodels used for testing

## How it all integrates together

![Diagram](/diagram.svg?raw=true&sanitize=true)
