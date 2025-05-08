using System.Collections.Immutable;
using Avalonia.Controls;
using PixiEditor.Extensions.UI.Overlays;

namespace PixiEditor.Extensions.FlyUI.Elements;

public class Expanded : SingleChildLayoutElement, IPropertyDeserializable
{
    private Control affectedControl;
    private int flex = 1;

    public int Flex
    {
        get => flex;
        set => SetField(ref flex, value);
    }

    public Expanded(LayoutElement? child = null, int flex = 1)
    {
        Child = child;
        Flex = flex;
    }

    public override Control BuildNative()
    {
        if (Child == null)
        {
            var control = new Control();
            ExpandedDecorator.SetFlex(control, Flex);
            affectedControl = control;
            return control;
        }

        var child = Child.BuildNative();
        ExpandedDecorator.SetFlex(child, Flex);
        affectedControl = child;

        return child;
    }

    protected override void AddChild(Control child)
    {
        if (child is not null)
        {
            affectedControl = child;
            ExpandedDecorator.SetFlex(child, Flex);
        }
    }

    protected override void RemoveChild()
    {
        if (affectedControl is not null)
        {
            ExpandedDecorator.SetFlex(affectedControl, 0);
            affectedControl = null;
        }
    }

    public IEnumerable<object> GetProperties()
    {
        yield return Flex;
    }

    public void DeserializeProperties(ImmutableList<object> values)
    {
        if (values.Count > 0)
        {
            Flex = (int)values[0];
        }
    }
}
