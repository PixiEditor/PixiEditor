using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ToolsViewModel : SubViewModel<ViewModelMain>
    {
        private Cursor toolCursor;

        public RelayCommand SelectToolCommand { get; set; } // Command that handles tool switching.

        public RelayCommand ChangeToolSizeCommand { get; set; }

        public Tool LastActionTool { get; private set; }

        public ObservableCollection<Tool> ToolSet { get; set; }

        public Cursor ToolCursor
        {
            get => toolCursor;
            set
            {
                toolCursor = value;
                RaisePropertyChanged("ToolCursor");
            }
        }

        public ToolsViewModel(ViewModelMain owner)
            : base(owner)
        {
            SelectToolCommand = new RelayCommand(SetTool, Owner.DocumentIsNotNull);
            ChangeToolSizeCommand = new RelayCommand(ChangeToolSize);
        }

        public void SetupTools(IServiceProvider services)
        {
            ToolSet = new ObservableCollection<Tool>(
                new ToolBuilder(services)
                .Add<MoveViewportTool>().Add<MoveTool>().Add<PenTool>().Add<SelectTool>().Add<MagicWandTool>().Add<FloodFill>()
                .Add<LineTool>().Add<CircleTool>().Add<RectangleTool>().Add<EraserTool>().Add<ColorPickerTool>()
                .Add<BrightnessTool>().Add<ZoomTool>()
                .Build());

            SetActiveTool<MoveViewportTool>();
        }

        public void SetActiveTool<T>()
            where T : Tool
        {
            SetActiveTool(typeof(T));
        }

        public void SetActiveTool(Tool tool)
        {
            Tool activeTool = ToolSet.FirstOrDefault(x => x.IsActive);
            if (activeTool != null)
            {
                activeTool.IsActive = false;
            }

            tool.IsActive = true;
            LastActionTool = Owner.BitmapManager.SelectedTool;
            Owner.BitmapManager.SetActiveTool(tool);
            SetToolCursor(tool.GetType());

            if (Owner.StylusSubViewModel != null)
            {
                Owner.StylusSubViewModel.ToolSetByStylus = false;
            }
        }

        public void SetTool(object parameter)
        {
            if (parameter is Type type)
            {
                SetActiveTool(type);
                return;
            }

            Tool tool = (Tool)parameter;
            SetActiveTool(tool.GetType());
        }

        private static T CreateTool<T>(IServiceProvider provider)
            where T : new()
        {
            T tool = default;
            Type toolType = typeof(T);

            foreach (PropertyInfo info in toolType.GetProperties(BindingFlags.Public))
            {
                if (!info.CanWrite)
                {
                    continue;
                }

                info.SetValue(tool, provider.GetService(info.PropertyType));
            }

            return tool;
        }

        private void ChangeToolSize(object parameter)
        {
            int increment = (int)parameter;
            int newSize = Owner.BitmapManager.ToolSize + increment;
            if (newSize > 0)
            {
                Owner.BitmapManager.ToolSize = newSize;
            }
        }

        private void SetActiveTool(Type toolType)
        {
            if (toolType == null && toolType.IsAssignableTo(typeof(Tool)))
            {
                return;
            }

            Tool foundTool = ToolSet.First(x => x.GetType() == toolType);
            SetActiveTool(foundTool);
        }

        private void SetToolCursor(Type tool)
        {
            if (tool != null)
            {
                ToolCursor = Owner.BitmapManager.SelectedTool.Cursor;
            }
            else
            {
                ToolCursor = Cursors.Arrow;
            }
        }
    }
}