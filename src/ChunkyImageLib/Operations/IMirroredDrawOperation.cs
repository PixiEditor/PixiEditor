namespace ChunkyImageLib.Operations;

internal interface IMirroredDrawOperation : IDrawOperation
{
    IDrawOperation AsMirrored(int? verAxisX, int? horAxisY);
}
