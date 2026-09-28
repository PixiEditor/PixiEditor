namespace PixiEditor.Models.ExternalServices;

public class NbpResponse
{
    public string Table { get; set; }
    public string Currency { get; set; }
    public string Code { get; set; }
    public List<NbpRate> Rates { get; set; }
}
