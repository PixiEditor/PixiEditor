namespace PixiEditor.Extensions.CommonApi.FlyUI;

public interface IMultiChildLayoutElement<TBuildResult> : ILayoutElement<TBuildResult>
{
    public List<ILayoutElement<TBuildResult>> Children { get; set; }
}
