using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Backend.Core;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Document;

public class TexturePreview : ObservableObject
{
    private Texture preview;

    public Guid Id { get; }

    public Guid? SubId { get; init; }

    public Texture Preview
    {
        get => preview;
        set
        {
            bool updated = SetProperty(ref preview, value);
            if(updated)
                InvokeTextureUpdated();
        }
    }

    public Dictionary<object, Func<VecI>> Listeners { get; } = new();

    public event Action TextureUpdated;

    private Action<Guid, Guid?> requestRender;

    public TexturePreview(Guid forId, Action<Guid> requestRender)
    {
        Id = forId;
        this.requestRender = (id, _) => requestRender(id);
    }

    public TexturePreview(Guid forId, Guid subId, Action<Guid, Guid?> requestRender)
    {
        Id = forId;
        SubId = subId;
        this.requestRender = requestRender;
    }

    public void Attach(object source, Func<VecI> getSize)
    {
        if(Listeners.TryAdd(source, getSize))
            requestRender(Id, SubId);
    }

    public void Detach(object source)
    {
        Listeners.Remove(source);
    }

    public void InvokeTextureUpdated()
    {
        TextureUpdated?.Invoke();
    }

    public VecI GetMaxListenerSize()
    {
        VecI maxSize = VecI.Zero;
        foreach (var sizeFunc in Listeners.Values)
        {
            VecI size = sizeFunc();
            if (size.Length > maxSize.Length)
            {
                maxSize = size;
            }
        }

        return maxSize;
    }
}
