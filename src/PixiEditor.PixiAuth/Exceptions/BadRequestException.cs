namespace PixiEditor.PixiAuth.Exceptions;

public class BadRequestException(string message) : PixiAuthException(400, message)
{

}
