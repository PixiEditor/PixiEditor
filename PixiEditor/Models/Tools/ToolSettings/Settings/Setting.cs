using PixiEditor.Helpers;
using System.Windows.Controls;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public abstract class Setting : NotifyableObject
    {
        public string Name { get; protected set; }
        public string Label { get; set; }
        public bool HasLabel => !string.IsNullOrEmpty(Label);
        private object value;
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

        public Setting(string name)
        {
            Name = name;
        }
    }
}
