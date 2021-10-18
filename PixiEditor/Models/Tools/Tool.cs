using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Extensions;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Layers;
using PixiEditor.Models.Position;
using PixiEditor.Models.Tools.ToolSettings;
using PixiEditor.Models.Tools.ToolSettings.Toolbars;
using PixiEditor.Models.Undo;

namespace PixiEditor.Models.Tools
{
    public abstract class Tool : NotifyableObject
    {
        public virtual string ToolName => GetType().Name.Replace("Tool", string.Empty);

        public virtual string DisplayName => ToolName.AddSpacesBeforeUppercaseLetters();

        public virtual string ImagePath => $"/Images/Tools/{ToolName}Image.png";

        public virtual bool HideHighlight { get; }

        public abstract string Tooltip { get; }

        public string ActionDisplay
        {
            get => actionDisplay;
            set
            {
                actionDisplay = value;
                RaisePropertyChanged("ActionDisplay");
            }
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                RaisePropertyChanged("IsActive");
            }
        }

        public Cursor Cursor { get; set; } = Cursors.Arrow;

        public Toolbar Toolbar { get; set; } = new EmptyToolbar();

        public bool CanStartOutsideCanvas { get; set; } = false;

        private bool isActive;
        private string actionDisplay = string.Empty;
        private StorageBasedChange _change;

        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }

        public virtual void AddUndoProcess(Document document)
        {
            var args = new object[] { _change.Document };
            document.UndoManager.AddUndoChange(_change.ToChange(UndoProcess, args));
            _change = null;
        }

        private void UndoProcess(Layer[] layers, UndoLayer[] data, object[] args)
        {
            if(args.Length > 0 && args[0] is Document document)
            {
                for (int i = 0; i < layers.Length; i++)
                {
                    Layer layer = layers[i];
                    document.Layers.RemoveAt(data[i].LayerIndex);

                    document.Layers.Insert(data[i].LayerIndex, layer);
                    if (data[i].IsActive)
                    {
                        document.SetMainActiveLayer(data[i].LayerIndex);
                    }
                }

            }
        }

        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }

        public virtual void OnKeyDown(KeyEventArgs e)
        {
        }

        public virtual void OnKeyUp(KeyEventArgs e)
        {
        }

        public virtual void OnStart(Coordinates clickPosition)
        {
        }

        public virtual void OnRecordingLeftMouseDown(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            Document doc = ViewModels.ViewModelMain.Current.BitmapManager.ActiveDocument;
            _change = new StorageBasedChange(doc, new[] { doc.ActiveLayer }, true);
        }

        public virtual void OnStoppedRecordingMouseUp(MouseEventArgs e)
        {
        }

        public virtual void OnMouseMove(MouseEventArgs e)
        {
        }

        public virtual void AfterAddedUndo(UndoManager undoManager)
        {
        }
    }
}