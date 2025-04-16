namespace PixiEditor.PixiAuth;

public class SessionModel
{
    public Guid SessionId { get; set; }
    public string SessionToken { get; set; } = string.Empty;

    public SessionModel(Guid sessionId, string sessionToken)
    {
        SessionId = sessionId;
        SessionToken = sessionToken;
    }
}
