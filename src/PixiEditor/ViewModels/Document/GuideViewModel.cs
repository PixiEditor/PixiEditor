using PixiEditor.ChangeableDocument.Actions.Generated;
using PixiEditor.Models.DocumentModels;
using PixiEditor.Models.Handlers;

namespace PixiEditor.ViewModels.Document;

internal class GuideViewModel : PixiObservableObject, IGuideHandler
{
    public Guid Id { get; }
    public bool IsVertical { get; }

    public double OffsetBindable
    {
        get => offset;
        set
        {
            var oldValue = offset;
            if (value == null && oldValue == null) return;
            if (oldValue != null && oldValue.Equals(value)) return;

            if (MergeChanges)
            {
                internals.ActionAccumulator.AddActions(new GuidePosition_Action(Id, value));
            }
            else
            {
                internals.ActionAccumulator.AddFinishedActions([new GuidePosition_Action(Id, value), new EndGuidePosition_Action()]);
            }
        }
    }

    private bool mergeChanges;

    public bool MergeChanges
    {
        get => mergeChanges;
        set
        {
            if (SetProperty(ref mergeChanges, value))
            {
                if (!value)
                {
                    internals.ActionAccumulator.AddFinishedActions(new EndGuidePosition_Action());
                }
            }
        }
    }

    private double offset;
    private DocumentInternalParts internals;

    public GuideViewModel(DocumentInternalParts internals, Guid id, bool isVerticalBindable, double offset)
    {
        Id = id;
        IsVertical = isVerticalBindable;
        this.offset = offset;
        this.internals = internals;
    }

    public void SetOffsetInternal(double newOffset)
    {
        this.offset = newOffset;
        OnPropertyChanged(nameof(OffsetBindable));
    }
}
