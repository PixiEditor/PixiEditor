using PixiEditor.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    public class FloatSetting : Setting
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public FloatSetting(string name, float initialValue, string label = "", 
            float min = float.NegativeInfinity, float max = float.PositiveInfinity) : base(name)
        {
            Label = label;
            Value = initialValue;
            Min = min;
            Max = max;
            SettingControl = GenerateNumberInput();
        }

        private NumberInput GenerateNumberInput()
        {
            NumberInput numbrInput = new NumberInput()
            {
                Width = 40,
                Height = 20,
                Min = Min,
                Max = Max,                

            };
            Binding binding = new Binding("Value")
            {
                Mode = BindingMode.TwoWay,
            };
            numbrInput.SetBinding(NumberInput.ValueProperty, binding);
            return numbrInput;
        }
    }
}
