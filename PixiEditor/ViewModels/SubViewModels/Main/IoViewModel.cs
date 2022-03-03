using PixiEditor.Helpers;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.Controllers.Shortcuts;
using PixiEditor.Models.Tools;
using PixiEditor.Models.Tools.Tools;
using System;
using System.Windows.Input;

namespace PixiEditor.ViewModels.SubViewModels.Main
{
    public class IoViewModel : SubViewModel<ViewModelMain>
    {
        public RelayCommand MouseMoveCommand { get; set; }

        public RelayCommand MouseDownCommand { get; set; }

        public RelayCommand PreviewMouseMiddleButtonCommand { get; set; }

        public RelayCommand MouseUpCommand { get; set; }

        public RelayCommand KeyDownCommand { get; set; }

        public RelayCommand KeyUpCommand { get; set; }

        private bool restoreToolOnKeyUp = false;

        private MouseInputFilter filter = new();
    
        public IoViewModel(ViewModelMain owner)
            : base(owner)
        {
            MouseDownCommand = new RelayCommand(filter.MouseDown);
            MouseMoveCommand = new RelayCommand(filter.MouseMove);
            MouseUpCommand = new RelayCommand(filter.MouseUp);
            PreviewMouseMiddleButtonCommand = new RelayCommand(OnPreviewMiddleMouseButton);
            GlobalMouseHook.OnMouseUp += filter.MouseUp;
            KeyDownCommand = new RelayCommand(OnKeyDown);
            KeyUpCommand = new RelayCommand(OnKeyUp);

            filter.OnMouseDown += OnMouseDown;
            filter.OnMouseMove += OnMouseMove;
            filter.OnMouseUp += OnMouseUp;
        }

        private void OnKeyDown(object parameter)
        {
            KeyEventArgs args = (KeyEventArgs)parameter;
            var key = args.Key;
            if (key == Key.System)
                key = args.SystemKey;

            ProcessShortcutDown(args.IsRepeat, key);

            if (Owner.BitmapManager.ActiveDocument != null)
            {
                Owner.BitmapManager.InputTarget.OnKeyDown(key);
            }

            HandleTransientKey(args, true);
        }

        private void HandleTransientKey(KeyEventArgs args, bool state)
        {
            var controller = Owner.ShortcutController;

            Key finalKey = args.Key;
            if (finalKey == Key.System)
            {
                finalKey = args.SystemKey;
            }

            if (controller.TransientShortcuts.ContainsKey(finalKey))
            {
                ChangeToolState(controller.TransientShortcuts[finalKey].GetType(), state);
            }
        }

        private void ProcessShortcutDown(bool isRepeat, Key key)
        {
            if (isRepeat && !restoreToolOnKeyUp && Owner.ShortcutController.LastShortcut != null &&
                Owner.ShortcutController.LastShortcut.Command == Owner.ToolsSubViewModel.SelectToolCommand)
            {
                restoreToolOnKeyUp = true;
                ShortcutController.BlockShortcutExection("ShortcutDown");
            }

            Owner.ShortcutController.KeyPressed(key, Keyboard.Modifiers);
        }

        private void OnKeyUp(object parameter)
        {
            KeyEventArgs args = (KeyEventArgs)parameter;
            var key = args.Key;
            if (key == Key.System)
                key = args.SystemKey;

            ProcessShortcutUp(key);

            if (Owner.BitmapManager.ActiveDocument != null)
                Owner.BitmapManager.InputTarget.OnKeyUp(key);

            HandleTransientKey(args, false);
        }

        private void ProcessShortcutUp(Key key)
        {
            if (restoreToolOnKeyUp && Owner.ShortcutController.LastShortcut != null &&
                Owner.ShortcutController.LastShortcut.ShortcutKey == key)
            {
                restoreToolOnKeyUp = false;
                Owner.ToolsSubViewModel.SetActiveTool(Owner.ToolsSubViewModel.LastActionTool);
                ShortcutController.UnblockShortcutExecution("ShortcutDown");
            }
        }

        private void OnMouseDown(object sender, MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                BitmapManager bitmapManager = Owner.BitmapManager;
                var activeDocument = bitmapManager.ActiveDocument;
                if (activeDocument == null)
                    return;

                bitmapManager.InputTarget.OnLeftMouseButtonDown(activeDocument.MouseXOnCanvas, activeDocument.MouseYOnCanvas);
            }
        }

        private void OnPreviewMiddleMouseButton(object sender)
        {
            ChangeToolState<MoveViewportTool>(true);
        }

        private void ChangeToolState<T>(bool setOn)
            where T : Tool
        {
            ChangeToolState(typeof(T), setOn);
        }

        private void ChangeToolState(Type type, bool setOn)
        {
            if (setOn)
            {
                var transientToolIsActive = Owner.ToolsSubViewModel.ActiveTool.GetType() == type;
                if (!transientToolIsActive)
                {
                    Owner.ToolsSubViewModel.SetActiveTool(type);
                    Owner.ToolsSubViewModel.ActiveToolIsTransient = true;
                }
            }
            else if (Owner.ToolsSubViewModel.LastActionTool != null && Owner.ToolsSubViewModel.ActiveToolIsTransient)
            {
                Owner.ToolsSubViewModel.SetActiveTool(Owner.ToolsSubViewModel.LastActionTool);
                restoreToolOnKeyUp = false;
                ShortcutController.UnblockShortcutExecution("ShortcutDown");
            }
        }

        private void OnMouseMove(object sender, EventArgs args)
        {
            var activeDocument = Owner.BitmapManager.ActiveDocument;
            if (activeDocument == null)
                return;
            Owner.BitmapManager.InputTarget.OnMouseMove(activeDocument.MouseXOnCanvas, activeDocument.MouseYOnCanvas);
        }

        private void OnMouseUp(object sender, MouseButton button)
        {
            if (Owner.BitmapManager.ActiveDocument == null)
                return;
            if (button == MouseButton.Left)
            {
                Owner.BitmapManager.InputTarget.OnLeftMouseButtonUp();
            }
            else if (button == MouseButton.Middle)
            {
                ChangeToolState<MoveViewportTool>(false);
            }
        }
    }
}
