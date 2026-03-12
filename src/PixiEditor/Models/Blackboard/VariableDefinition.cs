namespace PixiEditor.Models.Blackboard;

public class VariableDefinition
{
    public string Name { get; set; }
    public Type UnderlyingType { get; set; }
    public string? Unit { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }
    public VariableDefinition(string name, Type underlyingType)
    {
        Name = name;
        UnderlyingType = underlyingType;
    }

    public VariableDefinition(string name, Type underlyingType, string unit) : this(name, underlyingType)
    {
        Unit = unit;
    }

    public VariableDefinition(string name, Type underlyingType, string unit, double min, double max) : this(name, underlyingType, unit)
    {
        Min = min;
        Max = max;
    }
}

