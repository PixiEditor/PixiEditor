namespace PixiEditor.Extensions.CommonApi.LayoutBuilding;

public interface ITextElement<out TBuildResult> : ILayoutElement<TBuildResult>
{
    public string Data { get; set; }
}
