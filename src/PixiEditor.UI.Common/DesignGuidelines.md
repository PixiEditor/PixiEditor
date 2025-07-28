This project is dedicated for creating common PixiEditor styles and controls
using AvaloniaUI framework. It is used in PixiEditor to create consistent look along
all GUI applications.

## Structure

### Assets folder

This folder contains all common media files, like images, icons, fonts, etc.

### Styles folder

This folder contains all common styles/themes.

```PixiEditor.axaml``` is a default file that contains all common colors, geometry, etc.

### Controls folder

This folder contains all common controls, like buttons, sliders, etc.
Each control file has a definition of a control and a style for it. Each style
uses theme data taken from ```Styles/PixiEditor.axaml'``` file.