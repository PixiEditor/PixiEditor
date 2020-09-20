using System;
using System.Windows.Controls;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public abstract class Setting : NotifyableObject
    {
        public string Name { get; protected set; }
        public string Label { get; set; }
        public bool HasLabel => !string.IsNullOrEmpty(Label);

        public object Value
        {
            get => value;
            set
            {
                this.value = value;
                RaisePropertyChanged("Value");
            }
        }

        public Control SettingControl { get; set; }
        private object value;

        public Setting(string name)
        {
            Name = name;
        }
    }
    public class Setting<T> : Setting
    {
        public Setting(T value, string name) : base(name)
        {
            Name = name;
            this.Value = value;
        }

        public new T Value { get; set; }
    }
}