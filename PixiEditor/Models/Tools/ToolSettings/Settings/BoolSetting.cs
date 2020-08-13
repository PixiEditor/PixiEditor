using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using System;
using System.Reactive.Subjects;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class BoolSetting : Setting
    {
        public BoolSetting(string name, string label = "") : base(name)
        {
            Label = label;
            Value = false;
            SettingControl = GenerateCheckBox();
        }

        public BoolSetting(string name, bool isChecked, string label = "") : base(name)
        {
            Label = label;
            Value = isChecked;
            SettingControl = GenerateCheckBox();
        }

        private Control GenerateCheckBox()
        {
            var source = new Subject<bool>();
            CheckBox checkBox = new CheckBox
            {
                IsChecked = (bool)Value,
                VerticalAlignment = VerticalAlignment.Center,
                [!ToggleButton.IsCheckedProperty] = source.ToBinding()
            };

            return checkBox;
        }
    }
}