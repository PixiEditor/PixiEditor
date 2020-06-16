using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Behaviours;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class SizeSetting : Setting
    {
        public SizeSetting(string name, string label = null) : base(name)
        {
            Value = 1;
            SettingControl = GenerateTextBox();
            Label = label;
        }

        private TextBox GenerateTextBox()
        {
            TextBox tb = new TextBox
            {
                Style = Application.Current.FindResource("DarkTextBoxStyle") as Style,
                TextAlignment = TextAlignment.Center,
                MaxLength = 4,
                Width = 40,
                Height = 20
            };
            Binding binding = new Binding("Value")
            {
                Converter = new ToolSizeToIntConverter(),
                Mode = BindingMode.TwoWay
            };
            tb.SetBinding(TextBox.TextProperty, binding);
            TextBoxFocusBehavior behavor = new TextBoxFocusBehavior
            {
                FillSize = true
            };
            Interaction.GetBehaviors(tb).Add(behavor);
            return tb;
        }
    }
}