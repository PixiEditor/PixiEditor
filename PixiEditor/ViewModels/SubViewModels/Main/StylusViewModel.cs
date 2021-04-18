using System.Windows;
using System.Windows.Input;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class StylusViewModel : SubViewModel<ViewModelMain>
    {
        public bool ToolSetByStylus { get; set; }

        private Tool PreviousTool { get; set; }

        public StylusViewModel(ViewModelMain owner)
            : base(owner)
        {
            Window mw = Application.Current.MainWindow;

            mw.PreviewStylusButtonDown += Mw_StylusButtonDown;
            mw.PreviewStylusButtonUp += Mw_StylusButtonUp;
        }

        private void Mw_StylusButtonDown(object sender, StylusButtonEventArgs e)
        {
            e.Handled = true;

            if (e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
            {
                PreviousTool = Owner.BitmapManager.SelectedTool;
                Owner.ToolsSubViewModel.SetActiveTool<EraserTool>();
                ToolSetByStylus = true;
            }
        }

        private void Mw_StylusButtonUp(object sender, StylusButtonEventArgs e)
        {
            e.Handled = true;

            if (ToolSetByStylus && e.StylusButton.Guid == StylusPointProperties.TipButton.Id && e.Inverted)
            {
                Owner.ToolsSubViewModel.SetActiveTool(PreviousTool);
            }
        }
    }
}