﻿using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Models.Handlers;
using PixiEditor.Numerics;

namespace PixiEditor.ViewModels.Document;

public class SnappingViewModel : PixiObservableObject, ISnappingHandler
{
    public SnappingController SnappingController { get; } = new SnappingController();

    public SnappingViewModel()
    {
        SnappingController.AddXYAxis("Root", VecD.Zero);
    }

    public void AddFromDocumentSize(VecD documentSize)
    {
        SnappingController.AddXYAxis("DocumentSize", documentSize);
        SnappingController.AddXYAxis("DocumentCenter", documentSize / 2);
    }

    public void AddFromBounds(string id, Func<RectD> tightBounds)
    {
        SnappingController.AddBounds(id, tightBounds);
    }

    public void Remove(string id)
    {
        SnappingController.RemoveAll(id);
    }
}