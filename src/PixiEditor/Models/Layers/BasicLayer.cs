using PixiEditor.Helpers;
using System;

namespace PixiEditor.Models.Layers;

[Serializable]
public class BasicLayer : NotifyableObject, IHasGuid
{
    private int height;

    private int width;

    public int Width
    {
        get => width;
        set
        {
            width = value;
            RaisePropertyChanged("Width");
        }
    }

    public int Height
    {
        get => height;
        set
        {
            height = value;
            RaisePropertyChanged("Height");
        }
    }

    public Guid GuidValue { get; protected set; }
}