using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using PixiEditor.Models.Undo;
using PixiEditor.ViewModels;

namespace PixiEditor.Models.Controllers
{
    [DebuggerDisplay("{UndoStack.Count} undo steps, {RedoStack.Count} redo step(s)")]
    public class UndoManager : IDisposable
    {
        private bool lastChangeWasUndo;

        private PropertyInfo newUndoChangeBlockedProperty;

        public Stack<Change> UndoStack { get; set; } = new Stack<Change>();

        public Stack<Change> RedoStack { get; set; } = new Stack<Change>();

        public bool CanUndo => UndoStack.Count > 0;

        public bool CanRedo => RedoStack.Count > 0;

        public object MainRoot { get; set; }

        public UndoManager()
        {
            if (ViewModelMain.Current != null && ViewModelMain.Current.UndoSubViewModel != null)
            {
                MainRoot = ViewModelMain.Current.UndoSubViewModel;
            }
        }

        public UndoManager(object mainRoot)
        {
            MainRoot = mainRoot;
        }

        /// <summary>
        ///     Adds property change to UndoStack.
        /// </summary>
        public void AddUndoChange(Change change, bool invokedInsideSetter = false)
        {
            if (change.Property != null && (ChangeIsBlockedProperty(change) && invokedInsideSetter == true))
            {
                newUndoChangeBlockedProperty = null;
                return;
            }

            lastChangeWasUndo = false;

            // Clears RedoStack if last move wasn't redo or undo and if redo stack is greater than 0.
            if (lastChangeWasUndo == false && RedoStack.Count > 0)
            {
                RedoStack.Clear();
            }

            change.Root ??= MainRoot;
            UndoStack.Push(change);
        }

        /// <summary>
        ///     Sets top property in UndoStack to Old Value.
        /// </summary>
        public void Undo()
        {
            lastChangeWasUndo = true;
            Change change = UndoStack.Pop();
            if (change.ReverseProcess == null)
            {
                SetPropertyValue(GetChangeRoot(change), change.Property, change.OldValue);
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
        public void Redo()
        {
            lastChangeWasUndo = true;
            Change change = RedoStack.Pop();
            if (change.Process == null)
            {
                SetPropertyValue(GetChangeRoot(change), change.Property, change.NewValue);
            }
            else
            {
                change.Process(change.ProcessArguments);
            }

            UndoStack.Push(change);
        }

        /// <summary>
        /// Merges multiple undo changes into one.
        /// </summary>
        /// <param name="amount">Amount of changes to squash.</param>
        public void SquashUndoChanges(int amount)
        {
            string description = UndoStack.ElementAt(UndoStack.Count - amount).Description;
            SquashUndoChanges(amount, description);
        }

        /// <summary>
        /// Merges multiple undo changes into one.
        /// </summary>
        /// <param name="amount">Amount of changes to squash.</param>
        /// <param name="description">Final change description.</param>
        public void SquashUndoChanges(int amount, string description)
        {
            Change[] changes = new Change[amount];
            for (int i = 0; i < amount; i++)
            {
                changes[i] = UndoStack.Pop();
            }

            Action<object[]> reverseProcess = (object[] props) =>
            {
                foreach (var prop in props)
                {
                    Change change = (Change)prop;
                    if (change.ReverseProcess == null)
                    {
                        SetPropertyValue(GetChangeRoot(change), change.Property, change.OldValue);
                    }
                    else
                    {
                        change.ReverseProcess(change.ReverseProcessArguments);
                    }
                }
            };

            Action<object[]> process = (object[] props) =>
            {
                foreach (var prop in props.Reverse())
                {
                    Change change = (Change)prop;
                    if (change.Process == null)
                    {
                        SetPropertyValue(GetChangeRoot(change), change.Property, change.NewValue);
                    }
                    else
                    {
                        change.Process(change.ProcessArguments);
                    }
                }
            };

            Change change = new(reverseProcess, changes, process, changes, description);
            AddUndoChange(change);
        }

        public void Dispose()
        {
            foreach (Change change in UndoStack.Concat(RedoStack))
            {
                change.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        private bool ChangeIsBlockedProperty(Change change)
        {
            return (change.Root != null || change.FindRootProcess != null)
                && GetProperty(GetChangeRoot(change), change.Property).Item1 == newUndoChangeBlockedProperty;
        }

        private object GetChangeRoot(Change change)
        {
            return change.FindRootProcess != null ? change.FindRootProcess(change.FindRootProcessArgs) : change.Root;
        }

        private void SetPropertyValue(object target, string propName, object value)
        {
            var properties = GetProperty(target, propName);
            PropertyInfo propertyToSet = properties.Item1;
            newUndoChangeBlockedProperty = propertyToSet;
            propertyToSet.SetValue(properties.Item2, value, null);
        }

        /// <summary>
        /// Gets property info for propName from target. Supports '.' format.
        /// </summary>
        /// <param name="target">A object where target can be found.</param>
        /// <param name="propName">Name of property to get, supports nested property.</param>
        /// <returns>PropertyInfo about property and target object where property can be found.</returns>
        private Tuple<PropertyInfo, object> GetProperty(object target, string propName)
        {
            string[] bits = propName.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i]);
                target = propertyToGet.GetValue(target, null);
            }

            return new Tuple<PropertyInfo, object>(target.GetType().GetProperty(bits.Last()), target);
        }
    }
}