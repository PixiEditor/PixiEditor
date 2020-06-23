using System;

namespace PixiEditor.Models.DataHolders
{
    [Serializable]
    public class Change
    {
        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public string Description { get; set; }

        public string Property { get; set; }

        public Action<object[]> ReverseProcess { get; set; }
        public Action<object[]> Process { get; set; }
        public object[] ProcessArguments;
        public object[] ReverseProcessArguments;
        public object Root { get; set; }


        public Change(string property, object oldValue, object newValue, string description = "", object root = null)
        {
            Property = property;
            OldValue = oldValue;
            Description = description;
            NewValue = newValue;
            Root = root;
        }

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

        public Change(Action<object[]> reverseProcess, object[] reverseArguments,
            Action<object[]> process, object[] processArguments, string description = "")
        {
            ReverseProcess = reverseProcess;
            ReverseProcessArguments = reverseArguments;
            Process = process;
            ProcessArguments = processArguments;
            Description = description;
        }
    }
}