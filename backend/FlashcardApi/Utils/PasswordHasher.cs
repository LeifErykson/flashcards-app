namespace FlashcardApi.Utils;

public static class PasswordHasher
{
    // Hash a password with BCrypt (auto-generates salt)
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    
    // Verify a password against a hash
    public static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
