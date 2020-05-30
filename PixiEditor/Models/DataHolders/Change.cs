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

        public Action<object[]> ReverseProcess { get; set; } = null;
        public object[] ReverseProcessArguments;

        public Change(string property, object oldValue, object newValue, string description = "")
        {
            Property = property;
            OldValue = oldValue;
            Description = description;
            NewValue = newValue;
        }

        public Change(string property, Action<object[]> reverseProcess, object[] reverseArguments, 
            object newValue, string description = "")
        {
            Property = property;
            ReverseProcess = reverseProcess;
            ReverseProcessArguments = reverseArguments;
            NewValue = newValue;
            Description = description;
        }

        public Change()
        {

        }

    }
}
