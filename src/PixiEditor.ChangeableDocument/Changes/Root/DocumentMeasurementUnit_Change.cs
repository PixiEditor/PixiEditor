namespace PixiEditor.ChangeableDocument.Changes.Root;

internal class DocumentMeasurementUnit_Change : Change
{
    private MeasurementUnit? unit;
    private MeasurementUnit oldMeasurementUnit;

    [GenerateMakeChangeAction]
    public DocumentMeasurementUnit_Change(MeasurementUnit measurementUnit)
    {
        unit = measurementUnit;
    }

    public override bool InitializeAndValidate(Document target)
    {
        oldMeasurementUnit = target.MeasurementUnit;
        return unit != null;
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Apply(Document target, bool firstApply, out bool ignoreInUndo)
    {
        target.MeasurementUnit = unit;
        ignoreInUndo = false;

        return new DocumentMeasurement_ChangeInfo(target.MeasurementUnit);
    }

    public override OneOf<None, IChangeInfo, List<IChangeInfo>> Revert(Document target)
    {
        target.MeasurementUnit = oldMeasurementUnit;
        return new DocumentMeasurement_ChangeInfo(oldMeasurementUnit);
    }

    public override bool IsMergeableWith(Change other)
    {
        return other is DocumentMeasurementUnit_Change;
    }
}
