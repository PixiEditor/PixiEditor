using System.Windows.Controls;
using PixiEditor.Helpers;

namespace PixiEditor.Models.Tools.ToolSettings.Settings
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "StyleCop.CSharp.MaintainabilityRules",
        "SA1402:File may only contain a single type",
        Justification = "Same class with generic value")]
    public abstract class Setting<T> : Setting
    {
        protected Setting(string name)
            : base(name)
        {
        }

        public new T Value
        {
            get => (T)base.Value;
            set
            {
                base.Value = value;
                RaisePropertyChanged("Value");
            }
        }
    }

    public abstract class Setting : NotifyableObject
    {
        protected Setting(string name)
        {
            Name = name;
        }

        public object Value { get; set; }

        public string Name { get; }

        public string Label { get; set; }

        public bool HasLabel => !string.IsNullOrEmpty(Label);

        public Control SettingControl { get; set; }
    }
}