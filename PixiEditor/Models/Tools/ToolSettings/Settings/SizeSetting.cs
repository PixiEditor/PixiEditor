using PixiEditor.Helpers;
using PixiEditor.Helpers.Behaviours;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class SizeSetting : Setting
    {
        public SizeSetting(string name) : base(name)
        {
            Value = 1;
            SettingControl = GenerateTextBox();
        }

        private TextBox GenerateTextBox()
        {
            TextBox tb = new TextBox()
            {
                Style = Application.Current.FindResource("DarkTextBoxStyle") as Style,
                TextAlignment = TextAlignment.Center,
                MaxLength = 4,
                Width = 40,
            };
            //TextBoxNumericFinisherBehavior behavor = new TextBoxNumericFinisherBehavior();
            //Interaction.GetBehaviors(tb).Add(behavor);
            Binding binding = new Binding("Value")
            {
                Converter = new ToolSizeToIntConverter(),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            };
            tb.SetBinding(TextBox.TextProperty, binding);
            return tb;
        }
    }
}
