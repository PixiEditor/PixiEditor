namespace PixiEditor.PixiAuth;

public class ResendActivationModel
{
    public string Email { get; set; }

    public Guid SessionId { get; set; }

    public ResendActivationModel(string email, Guid sessionId)
    {
        Email = email;
        SessionId = sessionId;
    }
}
