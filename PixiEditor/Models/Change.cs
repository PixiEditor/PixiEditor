using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixiEditor.Models
{
    public class Change
    {
        private object _oldValue;

        public object OldValue
        {
            get { return _oldValue; }
            set { _oldValue = value; }
        }

        private object _newValue;

        public object NewValue
        {
            get { return _newValue; }
            set { _newValue = value; }
        }

        private object _type;

        public object Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private string _description;

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        private string _property;

        public string Property
        {
            get { return _property; }
            set { _property = value; }
        }


        public Change(string property, object oldValue, object newValue, string description = null)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
            Description = description;
        }

    }
}
