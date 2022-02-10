using PixiEditor.Models.Position;
using PixiEditor.ViewModels.SubViewModels.Main;
using System.Collections.Generic;
using System.Windows.Input;

namespace PixiEditor.Models.Tools.Tools
{
    public class MoveViewportTool : ReadonlyTool
    {
        private ToolsViewModel ToolsViewModel { get; }
        
        public MoveViewportTool(ToolsViewModel toolsViewModel)
        {
            Cursor = Cursors.SizeAll;
            ActionDisplay = "Click and move to pan viewport.";

            ToolsViewModel = toolsViewModel;
        }

        public override bool HideHighlight => true;
        public override string Tooltip
        {
            get 
            {
                return $"Move viewport. ({ShortcutKey})"; 
            }
        }

        public override void Use(IReadOnlyList<Coordinates> pixels)
        {
            // Implemented inside Zoombox.xaml.cs
        }
    }
}
