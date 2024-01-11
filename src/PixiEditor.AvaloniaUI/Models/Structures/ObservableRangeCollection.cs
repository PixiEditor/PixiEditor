﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;

namespace PixiEditor.AvaloniaUI.Models.Structures;

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