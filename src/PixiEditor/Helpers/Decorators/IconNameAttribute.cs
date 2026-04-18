namespace PixiEditor.Helpers.Decorators;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class IconNameAttribute : Attribute
{
    public string IconName { get; }

    public IconNameAttribute(string iconName)
    {
        IconName = iconName;
    }
}
