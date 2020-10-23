using System.Collections.Generic;
using System.Linq;
using PixiEditor.Models.DataHolders;

namespace PixiEditor.Models.Controllers
{
    public static class UndoManager
    {
        private static bool lastChangeWasUndo;

        public static Stack<Change> UndoStack { get; set; } = new Stack<Change>();

        public static Stack<Change> RedoStack { get; set; } = new Stack<Change>();

        public static bool CanUndo => UndoStack.Count > 0;

        public static bool CanRedo => RedoStack.Count > 0;

        public static object MainRoot { get; set; }

        /// <summary>
        ///     Sets object(root) in which undo properties are stored.
        /// </summary>
        /// <param name="root">Parent object.</param>
        public static void SetMainRoot(object root)
        {
            MainRoot = root;
        }

        /// <summary>
        ///     Adds property change to UndoStack.
        /// </summary>
        public static void AddUndoChange(Change change)
        {
            // Clears RedoStack if las move wasn't redo or undo and if redo stack is greater than 0
            if (lastChangeWasUndo == false && RedoStack.Count > 0)
            {
                RedoStack.Clear();
            }

            lastChangeWasUndo = false;
            change.Root ??= MainRoot;
            UndoStack.Push(change);
        }

        /// <summary>
        ///     Sets top property in UndoStack to Old Value.
        /// </summary>
        public static void Undo()
        {
            lastChangeWasUndo = true;
            Change change = UndoStack.Pop();
            if (change.ReverseProcess == null)
            {
                SetPropertyValue(change.Root, change.Property, change.OldValue);
            }
            else
            {
                change.ReverseProcess(change.ReverseProcessArguments);
            }

            RedoStack.Push(change);
        }

        /// <summary>
        ///     Sets top property from RedoStack to old value.
        /// </summary>
        public static void Redo()
        {
            lastChangeWasUndo = true;
            Change change = RedoStack.Pop();
            if (change.Process == null)
            {
                SetPropertyValue(change.Root, change.Property, change.NewValue);
            }
            else
            {
                change.Process(change.ProcessArguments);
            }

            UndoStack.Push(change);
        }

        private static void SetPropertyValue(object target, string propName, object value)
        {
            string[] bits = propName.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                System.Reflection.PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }

            System.Reflection.PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last());
            propertyToSet.SetValue(target, value, null);
        }
    }
}