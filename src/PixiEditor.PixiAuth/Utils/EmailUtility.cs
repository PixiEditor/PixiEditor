namespace PixiEditor.PixiAuth.Utils;

public static class EmailUtility
{
    public static string GetEmailHash(string email)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(email.ToLower());
        byte[] hashBytes = sha256.ComputeHash(inputBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
