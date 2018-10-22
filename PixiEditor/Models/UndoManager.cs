using PixiEditor.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace PixiEditor.Models
{
    public static class UndoManager
    {
        public static StackEx<Change> UndoStack { get; set; } = new StackEx<Change>(); 
        public static StackEx<Change> RedoStack { get; set; } = new StackEx<Change>();
        private static bool _stopRecording = false; 
        private static List<Change> _recordedChanges = new List<Change>();
        private static int _stackLimit = 25;
        private static bool _lastChangeWasUndo = false;
        public static bool CanUndo
        {
            get
            {
                return UndoStack.Count > 0;
            }
        }
        public static bool CanRedo
        {
            get
            {
                return RedoStack.Count > 0;
            }
        }

        public static object MainRoot { get; set; }

        /// <summary>
        /// Sets object(root) in which undo properties are stored.
        /// </summary>
        /// <param name="root">Parent object.</param>
        public static void SetMainRoot(object root)
        {
            MainRoot = root;
        }

        /// <summary>
        /// Records changes, used to save multiple changes as one
        /// </summary>
        /// <param name="property">Record property name.</param>
        /// <param name="oldValue">Old change value.</param>
        /// <param name="newValue">New change value.</param>
        /// <param name="undoDescription">Description of change.</param>
        public static void RecordChanges(string property, object oldValue, object newValue, string undoDescription = null)
        {
            if (_stopRecording == false)
            {
                if (_recordedChanges.Count < 3)
                {
                    _recordedChanges.Add(new Change(property, oldValue, newValue, undoDescription));
                }
            }
        }

        /// <summary>
        /// Stops recording changes and saves it as one.
        /// </summary>
        public static void StopRecording()
        {
                _stopRecording = true;
            if (_recordedChanges.Count > 0)
            {
                Change changeToSave = _recordedChanges[0];
                AddUndoChange(changeToSave.Property, changeToSave.OldValue, changeToSave.NewValue, changeToSave.Description);
                _recordedChanges.Clear();
            }
            _stopRecording = false;
        }
        /// <summary>
        /// Adds property change to UndoStack
        /// </summary>
        /// <param name="property">Changed property name.</param>
        /// <param name="oldValue">Old value of property.</param>
        /// <param name="newValue">New value of property.</param>
        /// <param name="undoDescription">Description of change.</param>
        public static void AddUndoChange(string property, object oldValue, object newValue, string undoDescription = null)
        {
            if(_lastChangeWasUndo == false && RedoStack.Count > 0) //Cleares RedoStack if las move wasn't redo or undo and if redo stack is greater than 0
            {
                RedoStack.Clear();
            }
            _lastChangeWasUndo = false;
            UndoStack.Push(new Change(property, oldValue, newValue, undoDescription)); //Adds change to UndoStack
            if(UndoStack.Count > _stackLimit) //If Undostack hits limit, removes elements from beggining of stack
            {
                UndoStack.Remove(0);
            }
            Debug.WriteLine(UndoStack.Count + " " + RedoStack.Count);
        }
        /// <summary>
        /// Sets top property in UndoStack to Old Value
        /// </summary>
        public static void Undo()
        {
            _lastChangeWasUndo = true;
            PropertyInfo propInfo = MainRoot.GetType().GetProperty(UndoStack.Peek().Property);
            propInfo.SetValue(MainRoot, UndoStack.Peek().OldValue);
            RedoStack.Push(UndoStack.Pop());
            UndoStack.Pop();
        }

        /// <summary>
        /// Sets top property from RedoStack to old value
        /// </summary>
        public static void Redo()
        {
            _lastChangeWasUndo = true;
            PropertyInfo propinfo = MainRoot.GetType().GetProperty(RedoStack.Peek().Property);
            propinfo.SetValue(MainRoot, RedoStack.Pop().OldValue);
        }
    }
}
