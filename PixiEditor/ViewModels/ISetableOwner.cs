namespace PixiEditor.ViewModels
{
    public interface ISetableOwner<TOwner>
    {
        public void SetOwner(TOwner owner);
    }
}