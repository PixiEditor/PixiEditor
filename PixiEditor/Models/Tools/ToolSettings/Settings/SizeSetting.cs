using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Xaml.Interactivity;
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
                TextAlignment = TextAlignment.Center,
                MaxLength = 4,
                Width = 40,
                Height = 20
            };

            if (Application.Current != null)
            {
                tb.Styles.Add((Style)Application.Current.FindResource("DarkTextBoxStyle"));
            }

            Binding binding = new Binding("Value")
            {
                Converter = new ToolSizeToIntConverter(),
                Mode = BindingMode.TwoWay
            };
            tb.Bind(TextBox.TextProperty, binding);
            TextBoxFocusBehavior behavor = new TextBoxFocusBehavior
            {
                FillSize = true
            };
            Interaction.GetBehaviors(tb).Add(behavor);
            return tb;
        }
    }
}