namespace PixiEditor.Extensions.CommonApi.LayoutBuilding.State;

public interface IStatelessElement<out TBuild> : ILayoutElement<TBuild>
{
    public ILayoutElement<TBuild> Build();
}
