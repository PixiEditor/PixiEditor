# 2.1.1.4

Date: `12.05.2026`

## New things

- Added a dialog asking before overwriting non .pixi files
- Added color picker tool overlay (by [Artem Melnyk](https://github.com/MellKam))
- Added new toolset "Adjustments" with Blur Tool, Sharpen Tool, Smudge Tool and Brightness Tool
- Added Velocity Output to Stroke Info Node
- Added "Place Element" to file paths in the command search (by [CPK](https://github.com/CPKreu))
- Added Oversample input in Brush Output Node,
- Added Sample Size output to Stroke Info Node
- [ABI] Added an option to pass config to custom tools

## Improvements

- Improved Brightness tool, it now supports custom brushes as brightness masks.
- Improved UI of Extension Browser
- Improved the UI of extensions button in the tools picker
- Added a button in the top menu bar for easier access to the Extension Browser
- Changed the icons of the extensions buttons
- Replaced Founder's Pack onboarding step with featured extensions

## Fixes

- Fixed binding key modifiers as a single key (e.g. Ctrl, Shift, Alt) not working.
- Fixed undo/redo with single key shortcut triggering when typed with text tool
- Fixed an issue with preview scaling for different window sizes
- Fixed brush shape overlay not updating in some cases
- Fixed an issue with brush settings resetting on tool change and not synchronizing properly
- Fixed invalid previews for some nodes
- Fixed a crash when trying to open .svg file as reference layer
- Fixed non-scrollable tools picker
- Fixed a bug that caused some tools to become the pen tool with Shared Toolbar option enabled
- As always, fixed various crashes
