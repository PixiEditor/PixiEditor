# 2.1.2.1

Date: `28.07.2026`

## New things

- Added Color Ramp Node - by [Ghoti](https://github.com/Ghoti-tpt)
  ![ramp|684x500](https://forum.pixieditor.net/uploads/default/original/1X/5b688c90920915aa9efc4c5fbb37780fac892344.png)

- Added Vector Math Node - by [flabbet](https://github.com/flabbet)
- Added Sticky Note - by [Ghoti](https://github.com/Ghoti-tpt)

Available in 16 581 375 colors!

![PixiEditor.Desktop_1Rv4UgmCLE|607x500](https://forum.pixieditor.net/uploads/default/original/1X/a1db4d63c65983d04a731fb6e501cc44bc927fc8.png)

- Added Comment Node - by [Ghoti](https://github.com/Ghoti-tpt)

It is now possible to create custom colored zones with a comment text.

![PixiEditor.Desktop_7xKye1XQI8|690x222](https://forum.pixieditor.net/uploads/default/original/1X/f8368eb78a95198ba11b43f7f2b11aa89a831635.png)

- Added Find and Replace tool to Adjustments toolset - by [Ghoti](https://github.com/Ghoti-tpt)
- Added Swap Color tool to Adjustments toolset - by [Laxan](https://github.com/Laxan3000)
- Added Gradient Brush - by [AwsomeAnthony](https://github.com/AwsmeAnthony)
- Added more sample texture options in Stroke Info Node - by [flabbet](https://github.com/flabbet)
- Added reset symmetry line buttons - by [Spencer A](https://github.com/slaeschliman)
- Added Stamp property to Stroke Info Node - by [flabbet](https://github.com/flabbet)
- Added Color Mapping Node and palette input type - by [Ghoti](https://github.com/Ghoti-tpt)
- Added Random Node - by [flabbet](https://github.com/flabbet)
- Added Normalize Color Values to Separate and Combine Color Nodes - by [flabbet](https://github.com/flabbet)
- Added Palette and Decompose Palette Nodes - by [Ghoti](https://github.com/Ghoti-tpt)

## Improvements

- Selecting multiple vector layers and changing main color will now recolor all layers - by [flabbet](https://github.com/flabbet)
- Various optimizations for the Brush Engine - by [flabbet](https://github.com/flabbet)
- Improved localization debug window - by [Equbuxu](https://github.com/Equbuxu)
- Improved Swap Color tool and optimized its performance - by [Laxan3000](https://github.com/Laxan3000)
- Added velocity interpolation for faster strokes - by [flabbet](https://github.com/flabbet)
- Optimized Gradient tool performance - by [flabbet](https://github.com/flabbet)
- Added more blend modes to Merge Node - by [flabbet](https://github.com/flabbet)
- Colors in the Node Graph now use 0-1 range instead of 0-255 - by [Equbuxu](https://github.com/Equbuxu) and [flabbet](https://github.com/flabbet)
- Translated 100% of the app to Spanish - by [Matalya](https://github.com/Matalya)

## Fixes

- Fixed not being able to move layers when there was non-structure layer in between - by [flabbet](https://github.com/flabbet)
- Fixed a bug that caused new layer to create when multiple layers were selected and user clicked on the canvas - by [flabbet](https://github.com/flabbet)
- It is now possible to use Open from clipboard option when no document is opened - by [flabbet](https://github.com/flabbet)
- Fixed layer order reversing when dragging multiple layers on the very bottom of the layer list - by [flabbet](https://github.com/flabbet)
- Fixed creating array of arrays - by [Ghoti](https://github.com/Ghoti-tpt)
- Fixed auto-saves saving to temp folder instead of local app data folder - by [flabbet](https://github.com/flabbet)
- Debug recording graph will now save snapshot to temp folder instead current working directory - by [flabbet](https://github.com/flabbet)
- Fixed deleting a folder not deleting all nested folders - by [flabbet](https://github.com/flabbet)
- Added a guard to prevent crash when corrupted font is installed - by [flabbet](https://github.com/flabbet)
- Fixed various crashes when working with text - by [flabbet](https://github.com/flabbet)
- Fixed HSL and HSV modes not working for Separate and Combine Color Nodes - by [Equbuxu](https://github.com/Equbuxu) and [flabbet](https://github.com/flabbet)
- Fixed various crashes - by [flabbet](https://github.com/flabbet)