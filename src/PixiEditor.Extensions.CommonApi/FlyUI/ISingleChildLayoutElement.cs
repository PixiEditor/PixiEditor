namespace PixiEditor.Extensions.CommonApi.FlyUI;

public interface ISingleChildLayoutElement<TBuildResult> : ILayoutElement<TBuildResult>
{
    public ILayoutElement<TBuildResult> Child { get; set; }
}
