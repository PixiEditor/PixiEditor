using Microsoft.Extensions.DependencyInjection;
using PixiEditor.Helpers;
using PixiEditor.Models.Enums;
using PixiEditor.Models.Events;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using PixiEditor.Models.Tools.ToolSettings.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class ToolsViewModel : SubViewModel<ViewModelMain>
    {
        private Cursor toolCursor;
        private Tool activeTool;

        public RelayCommand SelectToolCommand { get; set; } // Command that handles tool switching.

        public RelayCommand ChangeToolSizeCommand { get; set; }

        public Tool LastActionTool { get; private set; }

        public Cursor ToolCursor
        {
            get => toolCursor;
            set
            {
                toolCursor = value;
                RaisePropertyChanged("ToolCursor");
            }
        }

        public Tool ActiveTool
        {
            get => activeTool;
            private set => SetProperty(ref activeTool, value);
        }

        public int ToolSize
        {
            get => ActiveTool.Toolbar.GetSetting<SizeSetting>("ToolSize") != null
            ? ActiveTool.Toolbar.GetSetting<SizeSetting>("ToolSize").Value
            : 1;
            set
            {
                if (ActiveTool.Toolbar.GetSetting<SizeSetting>("ToolSize") is SizeSetting toolSize)
                {
                    toolSize.Value = value;
                    Owner.BitmapManager.UpdateHighlightIfNecessary();
                }
            }
        }

        public IEnumerable<Tool> ToolSet { get; private set; }

        public event EventHandler<SelectedToolEventArgs> SelectedToolChanged;

        public ToolsViewModel(ViewModelMain owner)
            : base(owner)
        {
            SelectToolCommand = new RelayCommand(SetTool, Owner.DocumentIsNotNull);
            ChangeToolSizeCommand = new RelayCommand(ChangeToolSize);
        }

        public void SetupTools(IServiceProvider services)
        {
            ToolSet = services.GetServices<Tool>();
            SetActiveTool<PenTool>();

            Owner.BitmapManager.BitmapOperations.BitmapChanged += (_, _) => TriggerCacheOutdated();
            Owner.BitmapManager.DocumentChanged += BitmapManager_DocumentChanged;
        }

        public void SetActiveTool<T>()
            where T : Tool
        {
            SetActiveTool(typeof(T));
        }

        public void SetActiveTool(Tool tool)
        {
            if (ActiveTool != null)
            {
                activeTool.IsActive = false;
            }

            LastActionTool = ActiveTool;
            ActiveTool = tool;

            if (LastActionTool != ActiveTool)
                SelectedToolChanged?.Invoke(this, new SelectedToolEventArgs(LastActionTool, ActiveTool));

            //update new tool
            Owner.BitmapManager.UpdateActionDisplay();

            tool.IsActive = true;
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

        public void TriggerCacheOutdated()
        {
            foreach (Tool tool in ToolSet)
            {
                if (tool is ICachedDocumentTool cachedTool)
                {
                    cachedTool.DocumentChanged();
                }
            }
        }

        private void BitmapManager_DocumentChanged(object sender, Models.Events.DocumentChangedEventArgs e)
        {
            if (e.OldDocument != null)
            {
                e.OldDocument.DocumentSizeChanged -= Document_DocumentSizeChanged;
                e.OldDocument.LayersChanged -= Document_LayersChanged;
            }

            if (e.NewDocument != null)
            {
                e.NewDocument.DocumentSizeChanged += Document_DocumentSizeChanged;
                e.NewDocument.LayersChanged += Document_LayersChanged;
            }

            TriggerCacheOutdated();

            void Document_DocumentSizeChanged(object sender, Models.DataHolders.DocumentSizeChangedEventArgs e)
            {
                TriggerCacheOutdated();
            }

            void Document_LayersChanged(object sender, Models.Controllers.LayersChangedEventArgs e)
            {
                if (e.LayerChangeType is LayerAction.Add or LayerAction.Remove or LayerAction.Move)
                {
                    TriggerCacheOutdated();
                }
            }
        }

        private void ChangeToolSize(object parameter)
        {
            int increment = (int)parameter;
            int newSize = ToolSize + increment;
            if (newSize > 0)
            {
                ToolSize = newSize;
            }
        }

        private void SetActiveTool(Type toolType)
        {
            Tool foundTool = ToolSet.First(x => x.GetType() == toolType);
            SetActiveTool(foundTool);
        }

        private void SetToolCursor(Type tool)
        {
            if (tool != null)
            {
                ToolCursor = ActiveTool.Cursor;
            }
            else
            {
                ToolCursor = Cursors.Arrow;
            }
        }
    }
}
