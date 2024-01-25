namespace PixiEditor.Extensions.CommonApi.LayoutBuilding;

public interface ISingleChildLayoutElement<TBuildResult> : ILayoutElement<TBuildResult>
{
    public ILayoutElement<TBuildResult> Child { get; set; }
}
