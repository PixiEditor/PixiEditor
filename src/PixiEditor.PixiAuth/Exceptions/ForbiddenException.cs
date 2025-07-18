namespace PixiEditor.PixiAuth.Exceptions;

public class ForbiddenException(string message) : PixiAuthException(403, message)
{

}
