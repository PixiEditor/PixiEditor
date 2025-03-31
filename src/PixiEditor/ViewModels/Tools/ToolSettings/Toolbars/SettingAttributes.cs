using Drawie.Backend.Core.ColorsImpl;

namespace PixiEditor.ViewModels.Tools.ToolSettings.Toolbars;

public static class Settings
{
    public class PercentAttribute : SettingsAttribute
    {
        public float Min { get; set; } = 0;

        public float Max { get; set; } = 1;
        
        public PercentAttribute(string labelKey) : base(labelKey) { }

        public PercentAttribute(string labelKey, float defaultValue) : base(labelKey, defaultValue)
        {
            
        }
        
        public PercentAttribute(string labelKey, float defaultValue, float min, float max) : base(labelKey, defaultValue)
        {
            Min = min;
            Max = max;
        }
        
    }
    
    /// <summary>
    /// A toolbar setting of type <see cref="bool"/>
    /// </summary>
    public class BoolAttribute : SettingsAttribute
    {
        public BoolAttribute(string labelKey) : base(labelKey) { }

        public BoolAttribute(string labelKey, object defaultValue) : base(labelKey, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of any enum
    /// </summary>
    public class EnumAttribute : SettingsAttribute
    {
        public EnumAttribute(string labelKey) : base(labelKey) { }

        public EnumAttribute(string labelKey, object defaultValue) : base(labelKey, defaultValue) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="Color"/>
    /// </summary>
    public class ColorAttribute : SettingsAttribute
    {
        public ColorAttribute(string labelKey) : base(labelKey) { }

        public ColorAttribute(string labelKey, byte r, byte g, byte b) : base(labelKey, new Color(r, g, b)) { }
        
        public ColorAttribute(string labelKey, byte r, byte g, byte b, byte a) : base(labelKey, new Color(r, g, b, a)) { }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="float"/>
    /// </summary>
    public class FloatAttribute : SettingsAttribute
    {
        public float Min { get; set; } = float.NegativeInfinity;
        public float Max { get; set; } = float.PositiveInfinity;

        public FloatAttribute(string labelKey) : base(labelKey) { }

        public FloatAttribute(string labelKey, float defaultValue) : base(labelKey, defaultValue) { }
        public FloatAttribute(string labelKey, float defaultValue, float min, float max) : base(labelKey, defaultValue)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary>
    /// A toolbar setting of type <see cref="int"/>
    /// </summary>
    public class SizeAttribute : SettingsAttribute
    {
        public SizeAttribute(string labelKey, double defaultValue = 1) : base(labelKey, defaultValue) { }

        public double Min { get; set; } = 1;
    }

    /// <summary>
    /// Marks a setting to be inherited from the toolbar type
    /// </summary>
    public class InheritedAttribute : SettingsAttribute { }

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class SettingsAttribute : Attribute
    {
        public string Name { get; set; }
        
        public string Notify { get; set; }
        
        public bool ExposedByDefault { get; set; } = true;

        public SettingsAttribute() { }
        
        public SettingsAttribute(string labelKey)
        {
            LabelKey = labelKey;
        }

        public SettingsAttribute(string labelKey, object defaultValue)
        {
            LabelKey = labelKey;
            DefaultValue = defaultValue;
        }
        
        public readonly string LabelKey;

        public readonly object DefaultValue;
    }
}
