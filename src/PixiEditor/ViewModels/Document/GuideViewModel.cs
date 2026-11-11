using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class GuideViewModel : PixiObservableObject, IGuideHandler
{
    public Guid Id { get; }
    public bool IsVertical { get; }

    public double Offset => offset;

    private double offset;

    public GuideViewModel(Guid id, bool isVerticalBindable, double offset)
    {
        Id = id;
        IsVertical = isVerticalBindable;
        this.offset = offset;
    }

    public void SetOffsetInternal(double newOffset)
    {
        this.offset = newOffset;
        OnPropertyChanged(nameof(Offset));
    }
}
