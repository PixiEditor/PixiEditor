# 2.1.1.0

Date: `10.05.2026`

## New things

- Added a dialog asking before overwriting non .pixi files
- Added color picker tool overlay (by [Artem Melnyk](https://github.com/MellKam))
- Added new toolset "Adjustments" with Blur Tool, Sharpen Tool, Smudge Tool and Brightness Tool

## Improvements

- Improved Brightness tool, it now supports custom brushes as brightness masks.
- Improved UI of Extension Browser

## Fixes

- Fixed binding key modifiers as a single key (e.g. Ctrl, Shift, Alt) not working.
- Fixed undo/redo with single key shortcut triggering when typed with text tool
- Fixed an issue with preview scaling for different window sizes
- Fixed brush shape overlay not updating in some cases
- Fixed an issue with brush settings resetting on tool change and not synchronizing properly
- Fixed invalid previews for some nodes