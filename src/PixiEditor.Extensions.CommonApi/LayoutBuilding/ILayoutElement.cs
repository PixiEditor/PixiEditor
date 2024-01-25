namespace PixiEditor.Extensions.CommonApi.LayoutBuilding;

public interface ILayoutElement<out TBuildResult>
{
    public TBuildResult Build();
}
