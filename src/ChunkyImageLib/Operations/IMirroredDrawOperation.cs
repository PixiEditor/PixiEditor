namespace ChunkyImageLib.Operations;

internal interface IMirroredDrawOperation : IDrawOperation
{
    IDrawOperation AsMirrored(double? verAxisX, double? horAxisY);
}
