namespace PixiEditor.ViewModels;

public interface ISettableOwner<TOwner>
{
    public void SetOwner(TOwner owner);
}