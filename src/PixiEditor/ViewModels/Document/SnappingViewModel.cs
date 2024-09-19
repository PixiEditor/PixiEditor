using CommunityToolkit.Mvvm.ComponentModel;
using PixiEditor.Models.Controllers.InputDevice;
using PixiEditor.Numerics;

namespace PixiEditor.ViewModels.Document;

public class SnappingViewModel : PixiObservableObject
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
}
