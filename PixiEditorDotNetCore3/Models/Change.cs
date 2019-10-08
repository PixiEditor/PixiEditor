using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PixiEditorDotNetCore3.Models;

namespace PixiEditorDotNetCore3.Models
{
    [Serializable]
    public class Change
    {
        public object OldValue { get; set; }

        public object NewValue { get; set; }

        public string Description { get; set; }

        public string Property { get; set; }

        public Change(string property, object oldValue, string description = "")
        {
            Property = property;
            OldValue = oldValue;
            Description = description;
            NewValue = OldValue;
        }

        public Change()
        {
           
        }

    }
}
