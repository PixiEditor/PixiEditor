namespace PixiEditor.Models.ExceptionHandling;

public class NonCrashInfo(string catchLocation, string catchMember)
{
    public string CatchLocation { get; } = catchLocation;

    public string CatchMember { get; } = catchMember;
}
