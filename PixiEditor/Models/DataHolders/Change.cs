using System;

namespace PixiEditor.Models.DataHolders
{
    [Serializable]
    public class Change
    {
        public object[] ProcessArguments;
        public object[] ReverseProcessArguments;

        /// <summary>
        ///     Creates new change for property based undo system
        /// </summary>
        /// <param name="property">Name of property</param>
        /// <param name="oldValue">Old value of property</param>
        /// <param name="newValue">New value of property</param>
        /// <param name="description">Description of change</param>
        /// <param name="root">Custom root for finding property</param>
        public Change(string property, object oldValue, object newValue,
            string description = "", object root = null)
        {
            Property = property;
            OldValue = oldValue;
            Description = description;
            NewValue = newValue;
            Root = root;
        }

        /// <summary>
        ///     Creates new change for mixed reverse process based system with new value property based system
        /// </summary>
        /// <param name="property">Name of property, which new value will be applied to</param>
        /// <param name="reverseProcess">Method with reversing value process</param>
        /// <param name="reverseArguments">Arguments for reverse method</param>
        /// <param name="newValue">New value of property</param>
        /// <param name="description">Description of change</param>
        /// <param name="root">Custom root for finding property</param>
        public Change(string property, Action<object[]> reverseProcess, object[] reverseArguments,
            object newValue, string description = "", object root = null)
        {
            Property = property;
            ReverseProcess = reverseProcess;
            ReverseProcessArguments = reverseArguments;
            NewValue = newValue;
            Description = description;
            Root = root;
        }

        /// <summary>
        ///     Creates new change for reverse process based system
        /// </summary>
        /// <param name="reverseProcess">Method with reversing value process</param>
        /// <param name="reverseArguments">Arguments for reverse method</param>
        /// <param name="process">Method with reversing the reversed value</param>
        /// <param name="processArguments">Arguments for process method</param>
        /// <param name="description">Description of change</param>
        public Change(Action<object[]> reverseProcess, object[] reverseArguments,
            Action<object[]> process, object[] processArguments, string description = "")
        {
            ReverseProcess = reverseProcess;
            ReverseProcessArguments = reverseArguments;
            Process = process;
            ProcessArguments = processArguments;
            Description = description;
        }

        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public string Description { get; set; }

        public string Property { get; set; }

        public Action<object[]> ReverseProcess { get; set; }
        public Action<object[]> Process { get; set; }
        public object Root { get; set; }
    }
}