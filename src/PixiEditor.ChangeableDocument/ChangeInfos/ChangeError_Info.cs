namespace PixiEditor.ChangeableDocument.ChangeInfos;

public struct ChangeError_Info : IChangeInfo
{
    public string Message { get; }

    public ChangeError_Info(string message)
    {
        Message = message;
    }
}
