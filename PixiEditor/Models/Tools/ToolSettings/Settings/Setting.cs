using System.Windows.Controls;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public abstract class Setting : NotifyableObject
    {
        protected Setting(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public string Label { get; set; }

        public bool HasLabel => !string.IsNullOrEmpty(Label);

        public Control SettingControl { get; set; }
    }
}