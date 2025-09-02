using CommunityToolkit.Mvvm.ComponentModel;
using Drawie.Backend.Core;
using Drawie.Numerics;

namespace PixiEditor.ViewModels.Document;

public class TexturePreview : ObservableObject
{
    private Texture preview;

    public Guid Id { get; }

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

    public TexturePreview(Guid forId)
    {
        Id = forId;
    }

    public void Attach(object source, Func<VecI> getSize)
    {
        Listeners.TryAdd(source, getSize);
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
