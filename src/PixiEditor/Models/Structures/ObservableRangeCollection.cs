using System.Collections.Generic;
using Avalonia.Collections;

namespace PixiEditor.Models.Structures;

public class ObservableRangeCollection<T> : AvaloniaList<T>
{
    public ObservableRangeCollection()
    {
    }

    public ObservableRangeCollection(IEnumerable<T> collection) : base(collection)
    {
    }

    public void ReplaceRange(IEnumerable<T> collection)
    {
        Clear();
        AddRange(collection);
    }
}
