
using Avalonia.Controls;
using PixiEditor.Helpers;
using ReactiveUI;

namespace PixiEditor.Models.Tools.ToolSettings
{
    public abstract class Setting : ReactiveObject
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
                this.RaisePropertyChanged("Value");
            }
        }

        public Control SettingControl { get; set; }
        private object value;

        public Setting(string name)
        {
            Name = name;
        }
    }
}