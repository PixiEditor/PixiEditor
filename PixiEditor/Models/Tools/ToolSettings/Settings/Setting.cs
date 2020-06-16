using System.Windows.Controls;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public abstract class Setting : NotifyableObject
    {
        private object value;

        public Setting(string name)
        {
            Name = name;
        }

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
    }
}