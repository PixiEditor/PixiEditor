using AvalonDock.Layout;

namespace PixiEditor.Views
{
    internal class ColorPanelController
    {
        private LayoutAnchorable colorPickerPanel;
        private LayoutAnchorable colorSlidersPanel;
        private LayoutAnchorable smallColorPickerPanel;

        public ColorPanelController(LayoutAnchorable colorPickerPanel, LayoutAnchorable colorSlidersPanel, LayoutAnchorable smallColorPickerPanel)
        {
            this.colorPickerPanel = colorPickerPanel;
            this.colorSlidersPanel = colorSlidersPanel;
            this.smallColorPickerPanel = smallColorPickerPanel;
        }

        public void DeterminePanelsToDisplay()
        {
            if (/*SystemParameters.PrimaryScreenHeight < 1010*/true)
            {
                colorPickerPanel.IsVisible = false;
                colorSlidersPanel.IsVisible = true;
                smallColorPickerPanel.IsVisible = true;
            }
            else
            {
                colorSlidersPanel.IsVisible = false;
                smallColorPickerPanel.IsVisible = false;
            }
        }
    }
}
