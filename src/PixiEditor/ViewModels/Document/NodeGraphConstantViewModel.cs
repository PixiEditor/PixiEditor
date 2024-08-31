using PixiEditor.ViewModels.SubViewModels;

namespace PixiEditor.ViewModels.Document;

internal class NodeGraphConstantViewModel(Guid id, Type type) : ViewModelBase, INodeGraphConstantHandler
{
    private object valueBindable;

    public Guid Id { get; } = id;

    public string NameBindable => Id.ToString()[..5];

    public Type Type { get; } = type;

    public object ValueBindable
    {
        get => valueBindable;
        set => ViewModelMain.Current.NodeGraphManager.UpdateConstantValue((this, value));
    }

    public void SetValueInternal(object value) => SetProperty(ref valueBindable, value, nameof(ValueBindable));
}
