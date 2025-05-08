using PixiEditor.Extensions.CommonApi.FlyUI.Properties;

namespace PixiEditor.Extensions.Sdk.Api.FlyUI;

public class Icon : StatelessElement
{
    public string IconName { get; set; }
    public double Size { get; set; } = 16;
    public Color Color { get; set; } = Colors.White;

    public Icon(string iconName, double size = 16, Color? color = null)
    {
        IconName = iconName;
        Size = size;
        if (color != null)
            Color = color.Value;
    }

    public override CompiledControl BuildNative()
    {
        CompiledControl icon = new CompiledControl(UniqueId, "Icon");
        icon.AddProperty(IconName);
        icon.AddProperty(Size);
        icon.AddProperty(Color);

        BuildPendingEvents(icon);
        return icon;
    }
}
