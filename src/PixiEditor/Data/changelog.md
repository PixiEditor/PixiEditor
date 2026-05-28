# 2.1.1.5

Date: `28.05.2026`

## New things

- Added "Game Of Life" example file
- Added Slovenian language - by [Andrew Poženel](https://github.com/anderlli0053)
- Added Blending Paint Brush - by [Ghoti](https://github.com/Ghoti-tpt)

## Improvements
- Moved Autosaves location to local appdata
- Added Minor optimization for brushes with cached stamps

## Fixes

- Fixed an exception happening when using Modify Image zone - by [Ghoti](https://github.com/Ghoti-tpt)
- Fixed parsing invalid svg filter link
- Fixed a crash when connecting number to math mode socket
- Fixed a crash related to invalid file extension
- Fixed a bug which caused animation timer to divide by 0
- Fixed long filename bug with discord rich presence
- Fixed a case where app was unable to save as .pixi
- Fixed a crash when pasting an image to a frame without a cel
- Fixed node picker navigation bug (clicking on categories will bring the view to correct position)
- Fixed a case that required uninstalling extension twice to remove it
- Fixed various crash scenarios