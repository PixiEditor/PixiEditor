using PixiEditor.Models.DataHolders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PixiEditor.Models.Controllers
{
    public static class UndoManager
    {
        public static StackEx<Change> UndoStack { get; set; } = new StackEx<Change>();
        public static StackEx<Change> RedoStack { get; set; } = new StackEx<Change>();
        private static List<Change> _recordedChanges = new List<Change>();
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


        public static void AddUndoChange(Change change)
        {
            if (_lastChangeWasUndo == false && RedoStack.Count > 0) //Cleares RedoStack if las move wasn't redo or undo and if redo stack is greater than 0
            {
                RedoStack.Clear();
            }
            _lastChangeWasUndo = false;
            UndoStack.Push(change);
        }

        /// <summary>
        /// Adds property change to UndoStack
        /// </summary>
        /// <param name="property">Changed property name.</param>
        /// <param name="oldValue">Old value of property.</param>
        /// <param name="newValue">New value of property.</param>
        /// <param name="undoDescription">Description of change.</param>
        public static void AddUndoChange(string property, object oldValue, object newValue, string undoDescription = "")
        {
            if (_lastChangeWasUndo == false && RedoStack.Count > 0) //Cleares RedoStack if las move wasn't redo or undo and if redo stack is greater than 0
            {
                RedoStack.Clear();
            }
            _lastChangeWasUndo = false;
            UndoStack.Push(new Change(property, oldValue, newValue, undoDescription));
        }

        /// <summary>
        /// Sets top property in UndoStack to Old Value
        /// </summary>
        public static void Undo()
        {
            _lastChangeWasUndo = true;
            Change change = UndoStack.Pop();
            if (change.ReverseProcess == null)
            {
                SetPropertyValue(MainRoot, change.Property, change.OldValue);
            }
            else
            {
                change.ReverseProcess(change.ReverseProcessArguments);
            }
            RedoStack.Push(change);
        }

        /// <summary>
        /// Sets top property from RedoStack to old value
        /// </summary>
        public static void Redo()
        {
            _lastChangeWasUndo = true;
            Change change = RedoStack.Pop();
            SetPropertyValue(MainRoot, change.Property, change.NewValue);
            UndoStack.Push(change);

        }
       

        private static void SetPropertyValue(object target, string propName, object value)
        {
            string[] bits = propName.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }
    }
}
