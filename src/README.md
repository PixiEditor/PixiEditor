# A potential new architecture for PixiEditor's backend.

Decouples the state of a document from the UI.

## Development progress

- ChunkyImage
    - [x] Basic commited and preview chunk support
    - [x] Affected chunk infrastructure
    - [x] Size constraining
    - [x] Cloning
    - [x] Periodic cleaning of empty chunks
    - [x] ChunkPool multithreading support
    - [x] Dispose that returns borrowed chunks
    - [x] ChunkyImage finalizer that returns borrowed chunks
    - [x] GetLatestChunk resolution parameter
        - [x] Support for different chunk sizes in chunkpool
        - [x] Rendering for different chunk sizes
        - [x] Read only interface for Chunk
    - [x] CommitedChunkStorage (used to store chunks for undo)
        - [ ] Write chunks to the hard drive?
        - [ ] Compress chunks?
    - [x] Linear color space for blending
    - [ ] Tests for everything related to the operation queueing
    - Operations
        - [ ] Support for paints with different blending (replace vs. alpha compose)
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
        - [ ] Symmetry operation
        - Filters
            - [ ] Hue/Saturation/Value
            - [ ] Brightness/Contrast
            - [ ] Curves
            - [ ] Gradient mapping
            - [ ] Replace color
- ChangeableDocument/Renderer
    - [x] Basic Action->Change->ChangeInfo pipeline
    - Undo handling
        - [x] UpdateableChange class for changes requiring preview
        - [x] Handling for changes that don't change anything
        - [x] Dispose changes
        - [x] Dispose the entire document
        - [x] Basic undo stack infrastructure
        - [x] Ignored changes (changes that don't get recorded in undo)
        - [x] Clear undo/redo history
        - [x] Manually merge changes
        - [x] Auto-merge similar successive changes
        - [ ] Limit undo stack size (discard old changes)
    - [x] Basic Collect Actions -> Apply actions -> Render changes pipeline
    - Rendering
        - [x] Basic layer stack rendering
        - [x] Opacity/Visibility support
        - [x] Recursive rendering of folders
        - [x] Blending modes support
        - [ ] Clip to layer below support
        - [x] Layer mask support
        - [x] Low-res rendering
        - [x] Don't render chunks outside viewport
        - [x] Support for rendering a subset of the structure (for merging selected layers, referring to selected layer, etc.)
        - [ ] Caching for folders
        - [ ] Caching for everything below current layer
        - Rendering images for changes (tools requiring final image, merge layers, etc.)
            - [x] ChunkRenderer as a part of Document
        - [ ] Rendering of layer previews
        - [ ] Rendering of canvas previews
        - [x] Support for multiple viewports
    - Changes
        - [x] Layer structure changes
        - [x] Combine layers onto a single layer
        - [ ] Reference layer manipulation?
        - [ ] Resize canvas (basic done, anchors left)
        - [ ] Resize image
        - [ ] Draw image (for pasting/loading from file)
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
        - [x] Layer mask manipulation
- ViewModel
    - [ ] Action filtering
    - [x] Viewport movement as an action
    - [x] Integrate viewport from PixiEditor
    - [x] Rotate viewport
    - [x] Flip viewport
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
- UpdateableChange: A type of change that has `ApplyTemporarily` and `Update` functions. These can be called multiple times before the regular `Apply` function is called. `Revert` here should revert to a state before `ApplyTemporarily` was called for the first time. Used for changes that can be updated in real time, like the opacity slider.
- ChangeInfo: A piece of data describing the changes made to the state by a Change. It is returned by the `Apply` and `Revert` functions of the Change class.

Note that changes must not store any references to the Document, Layers, ChunkyImages, and other IChangeables. If any data needs to be stored for revert to work, it must be copied. If you need to refer to some layer, store it's GUID. Consider a situation where you create a layer, draw on it, undo twice, and redo twice. When you undo the layer gets deleted, and then recreated on redo. If the drawing change were to store a reference to the layer we'd get an exception when trying to redo it. It is possible to carefully implement the new layer operation in a way that preserves the reference, but it's a lot easier not to store references at all.

ChangeableDocument uses ChunkyImages to store layer bitmaps and to draw on them.

Generally, ChangeableDocument will be used in PixiEditor by pumping all user actions into it, getting ChangeInfos back and updating the UI based on them.

- ### StructureRenderer

The main purpose of StructureRenderer is rendering the final visible image from all the layer images. It has access to ChangeableDocument's state and also receives all of it's ChangeInfos. StructureRenderer updates the final visible image when it encounters one or more ChangeInfos that describe some visible change (drawing, canvas size change, etc.). StructureRenderer can use low-resolution version of chunks from ChunkyImages to speed up rendering, but only while a tool is in use. Once you've stopped using the tool it always renders the final full-res image. StructureRenderer also renders layer previews.

StructureRenderer emits it's own ChangeInfos to notify the UI about the changes to the final image. They mainly include dirty rectangles (just the coordinates, not the data) and requests to recreate WriteableBitmaps when canvas size changes.

During the implementation process StructureRenderer will most likely become a part of ChangeableDocument, with the final rendered image and layer previews becoming parts of the document state.

- ### PixiEditorPrototype

A mockup UI with viewmodels used for testing

## How it all integrates together

Solid lines denote control flow (A -> B means A calls B); Dashed lines denote data flow (A -> B means B accesses data stored in A)

![Diagram](/diagram.svg?raw=true&sanitize=true)

## Some notes on chunk compression, pipette tool, and the renderer cache

### Implementing chunk compression

When compressed, the chunks need to return their surface into the pool. Right now ChunkPool stores chunks, but with this change the ChunkPool will need to be converted into a SurfacePool. This also allows us to make Chunks return their surfaces to the pool in the finalizer (Right now the chunks that haven't been disposed are garbage collected instead of being returned into the pool, cause otherwise the chunks would return themselves into the pool while already being marked for garbage collection. This should still work in theory but seems too hacky).

The compression should happen on a separate thread. The general idea is to make a Chunk.Compress() function along with a chunk.MarkedForCompression flag. Every N seconds, we should spawn a thread (using a timer or something) that would loop over all existing chunks. The loop works like this: If a chunk isn't marked for compression, it gets marked. If the chunk is already marked for compression the Chunk.Compress() function is called. Whenever someone tries to read from or write into a chunk the MarkedForCompression flag gets reset. This ensures that only chunks that haven't been accessed for at least N seconds are compressed. If someone tries to access a compressed chunk it would get decompressed first. 

Right now Chunk.Surface is a public property, and anyone can directly do stuff with the surface. This property will need to become private, and all canvas drawing functions will need to be wrapped. The wrappers will reset the MarkedForCompression flag, decompress the surface if it's currently compressed, and enable thread safety. Thread safety is required because Chunk.Compress() is called from a separate thread. Also, thread safety will allow the any thread to access the chunks at any time, which is required for the pipette tool and for the renderer cache.

Since the compression thread needs to loop over all chunks the chunks need to be stored somewhere. A naive solution would be to add all chunks into a static ConcurrentBag<Chunk> and remove them on dispose. This will hovewer require all chunks to be explicitly disposed, so instead I propose using a static ConcurrentBag<WeakReference> where the weak reference points to the chunks. This will allow the chunks to be garbage collected. Any chunks that have been garbage collected can be removed from the list by the compression thread.

### Making ChunkyImage thread-safe

For the pipette tool and the renderer cache to work they need to be able to access the Layers' ChunkyImages directly at any time, even if they are currently being edited in a separate thread. Therefore, ChunkyImage must become thread-safe. For the most part it's just a matter of adding a lock statement to all public functions, but there is a catch. ChunkyImage.GetLatestChunk and ChunkyImage.GetCommitedChunk return the chunks that are used internally by the ChunkyImage. The chunks are hidden behind the IReadOnlyChunk interface which protects them from being messed with, but that's it. If any changes are made to the ChunkyImage all chunks previously returned by GetLatest/CommitedChunk become effectively invalid (they can be modified in any way or even returned into the pool). At the moment it isn't a problem because the ChunkyImages are only ever accessed by a single thread at once, and no one holds onto the chunks returned by the aforementioned functions.

~~Obviously, we'd need a different system to make ChunkyImage truly thread safe. A simple solution would be to make a copy of the chunks in GetLatest/CommitedChunk, but that would noticeably affect rendering performance. Instead, I propose a ChunkView class. A ChunkView can be created by calling Chunk.CreateView(). Internally, the Chunk will store a weak reference to the created ChunkView. ChunkView will have methods that let you read the surface of it's corresponding chunk and a Detach() method. The Detach() method will make the ChunkView copy the surface of it's chunk, store the copy internally, and get rid of the reference to the original chunk. Whenever a Chunk gets modified or disposed, it will call detach on all the ChunkViews it has a weak reference to, if they haven't been disposed or garbage collected already, and once it's done the Chunk will get rid of the reference. ChunkView will need to be thread safe. This mechanism will allow us to avoid the copying overhead most of the time while also ensuring that the chunkviews we get from ChunkyImage.GetLatest/CommitedChunk are always valid, even after the ChunkyImage is modified.~~

Edit: I decided that it would be much simpler to not return any chunks at all and instead add a couple of wrappers for `Chunk.Surface.SkiaSurface.Canvas.DrawSurface` into `ChunkyImage`.

### Implementing a cache for the WriteableBitmapUpdater

At the moment, WriteableBitmapUpdater receives IChangeInfos from ActionAccumulator along with the WriteableBitmap that needs to be updated. It then processes all the IChangeInfos and decides which chunks need to be redrawn based on them. The chunks are redrawn fully from scratch, starting from the bottom of the layer tree all the way to the top. This works just fine, but with enough layers it will get laggy. If you think about the normal drawing workflow, most of the time you draw many different things on a single layer before switching to another. This presents an easy optimization: pre-render all layers that come before the current one, and when the current layer is changed draw on top of the pre-rendered image. The same can't be done with the layers above the current one though, as depending on their blending mode, masks, and other parameters the different rendering order can result in a final image that looks different. Another optimization that can be done is pre-rendering the contents of folders, as they are fully independent from everything outside (unless the blending mode is set to "Through").

This pre-rendering process can be done in a separate thread inside WriteableBitmapUpdater. Whenever a chunk is rendered it will try to use pre-rendered images if they exist, and render from scratch otherwise. While processing the IChangeInfos the WriteableBitmapUpdater will get rid of the pre-rendered images if the layers they contain were modified. It will also give the rendering thread instruction about the stuff that needs to be pre-rendered, e.g. "the active layer just got changed, so please work on pre-rendering the layers that are below our new location" (this means that active layer change will need to become an action).

Note that the cache only gets updated after all the IChangeInfos are processed. This means that we can't use the cache inside ChangeableDocument as it can get outdated if multiple Actions get proccessed in a single batch. Hypothetically, we could integrate the cache into ChangeableDocument and update it after every change, but I believe the minor speed up for some operations (mainly just the fill bucket and the magic wand) won't be worth the added complexity. It's nice to keep the caching logic decoupled from ChangeableDocument.
