using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using PixiEditor.Helpers;
using PixiEditor.Helpers.Behaviours;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class SizeSetting : Setting<int>
    {
        public SizeSetting(string name, string label = null) : base(name)
        {
            Value = 1;
            SettingControl = GenerateTextBox();
            Label = label;
        }

        private TextBox GenerateTextBox()
        {
            var tb = new TextBox
            {
                TextAlignment = TextAlignment.Center,
                MaxLength = 4,
                Width = 40,
                Height = 20
            };

            if (Application.Current != null) tb.Style = (Style) Application.Current.TryFindResource("DarkTextBoxStyle");

            var binding = new Binding("Value")
            {
                Converter = new ToolSizeToIntConverter(),
                Mode = BindingMode.TwoWay
            };
            tb.SetBinding(TextBox.TextProperty, binding);
            var behavor = new TextBoxFocusBehavior
            {
                FillSize = true
            };
            Interaction.GetBehaviors(tb).Add(behavor);
            return tb;
        }
    }
}