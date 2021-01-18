using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Behaviours;
using PixiEditor.Views;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class SizeSetting : Setting<int>
    {
        public SizeSetting(string name, string label = null)
            : base(name)
        {
            Value = 1;
            SettingControl = GenerateTextBox();
            Label = label;
        }

        private SizeInput GenerateTextBox()
        {
            SizeInput tb = new SizeInput
            {
                Width = 40,
                Height = 20,
                FontSize = 12,
            };

            Binding binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
            };
            tb.SetBinding(SizeInput.SizeProperty, binding);
            return tb;
        }
    }
}