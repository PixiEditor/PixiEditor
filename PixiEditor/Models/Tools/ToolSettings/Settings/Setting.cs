using System.Windows.Controls;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public abstract class Setting<T> : NotifyableObject
    {
        public string Name { get; }

        public string Label { get; set; }

        public bool HasLabel => !string.IsNullOrEmpty(Label);

        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                RaisePropertyChanged("Value");
            }
        }

        public Control SettingControl { get; set; }

        private T value;

        protected Setting(string name)
        {
            Name = name;
        }
    }

    public abstract class Setting : Setting<object>
    {
        protected Setting(string name)
            : base(name)
        {
        }
    }
}