# 2.1.2.5

Date: `30.10.2026`

# Improvements

- Improved contrast of brush overlay - by [flabbet](https://github.com/flabbet)
- Improved shift-click straight line drawing - by [Equbuxu](https://github.com/Equbuxu)
- Scrolling over number inputs now requires focus, this prevents accidental changes - by [flabbet](https://github.com/flabbet)
- Scrolling inside the timeline, now scrolls towards the active frame instead of mouse position - by [flabbet](https://github.com/flabbet)
- Grid lines are now automatically 1/12 of the document size by default, instead of 1x1 px - by [flabbet](https://github.com/flabbet)
- Trying to delete a layer when selection on the canvas is active, will now delete only canvas contents instead the entire layer - by [flabbet](https://github.com/flabbet)

# Fixes

- Fixed Game of Life sample file - by [Ghoti](https://github.com/Ghoti-tpt)
- Fix Shift-clicking with the pixel-perfect brush setting renders the user unable to draw until the setting is disabled - by [Equbuxu](https://github.com/Equbuxu)
- Fix Line tool ends are not consistently aligned with the mouse when using shift to lock to 45 - by [Equbuxu](https://github.com/Equbuxu)
- Fix The beginning of the line is snapped to the corner of the pixel while the end of the line isn't snapped at all - by [Equbuxu](https://github.com/Equbuxu)
- Fixed layers in folders exporting duplicate SVG elements - by [flabbet](https://github.com/flabbet)
- Fixed layer manager always scrolling to selected layer - by [flabbet](https://github.com/flabbet)
- Fixed Find and Replace tool not working with 0 tolerance setting - by [flabbet](https://github.com/flabbet)
- Fixed layer not creating when trying to draw on a folder without a mask - by [flabbet](https://github.com/flabbet)
- Fixed an issue that required clicking twice to draw something on an automatically created layer - by [flabbet](https://github.com/flabbet)
- Fixed a bug, that caused blurry viewport when it was rotated 90 or 180 degrees - by [flabbet](https://github.com/flabbet)
- Fixed pasting an image into active layer not enabling editing overlay - by [flabbet](https://github.com/flabbet)