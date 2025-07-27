namespace PixiEditor.PixiAuth.Exceptions;

public class InternalServerErrorException(string message) : PixiAuthException(500, message)
{

}
