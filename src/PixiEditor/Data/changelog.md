# 2.2.0.4

Date: `27.10.2026`

- Scrolling in the timeline now scrolls towards the active frame instead of a mouse
- Fixed erasing text on big canvases leaving visual artifacts
- Fixed pasting an image to the active layer not enabling editing overlay
- Clicking "Delete" when document have active selection will always remove the selection content instead of removing the layer
- Added cel context menu
- Fixed pasting cels issues (overlapping cels, wrong contents)
- Added default auto sizing of grid lines so it always sets 1/12 of image size instead of being 1x1
- Cel group (layer) preview is now clipped to bounds, just like previews of normal layers

# 2.2.0.3

Date: `21.10.2026`

- Scrolling over number inputs now require focus
- Fixed crash when opening a login popup
- Fixed layer manager always scrolling to newly selected layer
- Fixed tool not drawing immediately on newly created layer when it was started on incompatible layer
- Fixed find and replace tool with 0 tolerance setting

# 2.2.0.2

Date: `20.10.2026`

# Improvements

- Reworked text editing, it is now possible to select a part of the text and edit its settings directly
- Reworked font picker UI
- Added favourite fonts filtering
- Pen tool shift key straight line overlay and snapping
- Undo/redo support for straight line drawing

# Fixes

- Fixed a crash when dragging layers on macOS
- Fixed not working drag and drop of palette colors
- Fix brush engine snap to pixels for negative coordinates
- Fixed folder not rendering in high dpi when the opacity is < 100%
- Fixed duplicate svg tags when exporting content with folders