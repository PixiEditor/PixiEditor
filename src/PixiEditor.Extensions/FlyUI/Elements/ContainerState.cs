namespace PixiEditor.Extensions.FlyUI.Elements;

public class ContainerState : State
{
    public LayoutElement Content { get; set; }

    public override LayoutElement BuildElement()
    {
        return Content;
    }
}
