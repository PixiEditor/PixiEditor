namespace PixiEditor.Extensions.CommonApi.LayoutBuilding;

public interface IMultiChildLayoutElement<TBuildResult> : ILayoutElement<TBuildResult>
{
    public List<ILayoutElement<TBuildResult>> Children { get; set; }
}
