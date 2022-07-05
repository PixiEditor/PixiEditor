namespace PixiEditor.ViewModels;

internal interface ISettableOwner<TOwner>
{
    public void SetOwner(TOwner owner);
}
