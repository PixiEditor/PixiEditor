using PixiEditor.Models.Position;
using PixiEditor.Models.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PixiEditor.Models.Controllers
{
    public class ToolSession
    {
        private List<Coordinates> mouseMovement = new();
        private bool ended = false;

        public IReadOnlyList<Coordinates> MouseMovement => mouseMovement;
        public Tool Tool { get; }

        public bool IsCtrlDown { get; private set; }
        public bool IsShiftDown { get; private set; }
        public bool IsAltDown { get; private set; }

        public ToolSession(
            Tool tool,
            double mouseXOnCanvas,
            double mouseYOnCanvas,
            IReadOnlyDictionary<Key, KeyStates> keyboardStates)
        {
            if (tool == null)
                throw new ArgumentNullException(nameof(tool));
            Tool = tool;

            Tool.Session = this;
            InvokeKeyboardEvents(keyboardStates);
            mouseMovement.Add(new((int)Math.Floor(mouseXOnCanvas), (int)Math.Floor(mouseYOnCanvas)));
            Tool.BeforeUse();
        }

        private void InvokeKeyboardEvents(IReadOnlyDictionary<Key, KeyStates> keyboardStates)
        {
            foreach (var pair in keyboardStates)
            {
                if (pair.Value == KeyStates.None)
                    OnKeyUp(pair.Key);
                else if (pair.Value == KeyStates.Down)
                    OnKeyDown(pair.Key);
            }
        }

        public void EndSession(IReadOnlyDictionary<Key, KeyStates> keyboardStates)
        {
            if (ended)
                throw new Exception("Session has ended already");
            ended = true;

            Tool.AfterUse();
            InvokeReleaseKeyboardEvents(keyboardStates);
            Tool.Session = null;
        }

        private void InvokeReleaseKeyboardEvents(IReadOnlyDictionary<Key, KeyStates> keyboardStates)
        {
            foreach (var pair in keyboardStates)
            {
                if (pair.Value == KeyStates.Down)
                    OnKeyUp(pair.Key);
            }
        }

        public void OnKeyDown(Key key)
        {
            if (key == Key.LeftCtrl)
                IsCtrlDown = true;
            else if (key == Key.LeftShift)
                IsShiftDown = true;
            else if (key == Key.LeftAlt)
                IsAltDown = true;

            Tool.OnKeyDown(key);
        }

        public void OnKeyUp(Key key)
        {
            if (key == Key.LeftCtrl)
                IsCtrlDown = false;
            else if (key == Key.LeftShift)
                IsShiftDown = false;
            else if (key == Key.LeftAlt)
                IsAltDown = false;

            Tool.OnKeyUp(key);
        }

        public void OnPixelPositionChange(Coordinates pos)
        {
            mouseMovement.Add(pos);
        }
    }
}
