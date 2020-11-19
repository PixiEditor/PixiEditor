namespace PixiEditor.ViewModels.SubViewModels
{
    public class SubViewModel<T> : ViewModelBase where T: ViewModelBase
    {
        public T Owner { get; }

        public SubViewModel(T owner)
        {
            Owner = owner;
        }        
    }
}
