namespace PixiEditor.Models.ExceptionHandling;

public class CrashedSessionInfo
{
    public Guid? AnalyticsSessionId { get; set; }
    
    public ICollection<CrashedFileInfo>? OpenedDocuments { get; set; }

    public CrashedSessionInfo()
    {
    }

    public CrashedSessionInfo(Guid? analyticsSessionId, ICollection<CrashedFileInfo> openedDocuments)
    {
        AnalyticsSessionId = analyticsSessionId;
        OpenedDocuments = openedDocuments;
    }
}
