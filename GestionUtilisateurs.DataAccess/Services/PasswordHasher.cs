using System.Security.Cryptography;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Service de hachage et de vérification des mots de passe.
/// Utilise l'algorithme PBKDF2 avec sel aléatoire (norme RFC 2898).
/// </summary>
public interface IPasswordHasher
{
    /// <summary>Hache un mot de passe en clair.</summary>
    string Hash(string password);

    /// <summary>Vérifie qu'un mot de passe en clair correspond à un hachage.</summary>
    bool Verify(string password, string storedHash);
}

/// <summary>
/// Implémentation de <see cref="IPasswordHasher"/> basée sur PBKDF2 (Rfc2898DeriveBytes).
/// Format du hachage : <c>$PBKDF2${iterations}${saltBase64}${hashBase64}</c>
/// </summary>
public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int Iterations = 100_000;
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

        return $"$PBKDF2${Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
            return false;

        var parts = storedHash.Split('$');
        if (parts.Length != 5 || parts[1] != "PBKDF2")
            return false;

        if (!int.TryParse(parts[2], out var iterations))
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[3]);
            var expectedHash = Convert.FromBase64String(parts[4]);

            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithm, expectedHash.Length);

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
