using GestionUtilisateurs.DataAccess.Services;
using Xunit;

namespace GestionUtilisateurs.Tests;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _hasher = new Pbkdf2PasswordHasher();

    [Fact]
    public void Hash_ReturnsNonEmptyStringWithPrefix()
    {
        var hash = _hasher.Hash("MonMotDePasse123");

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.StartsWith("$PBKDF2$", hash);
    }

    [Fact]
    public void Verify_ReturnsTrueForCorrectPassword()
    {
        var hash = _hasher.Hash("S3cur3-Passw0rd");

        Assert.True(_hasher.Verify("S3cur3-Passw0rd", hash));
    }

    [Fact]
    public void Verify_ReturnsFalseForWrongPassword()
    {
        var hash = _hasher.Hash("Correct-Password");

        Assert.False(_hasher.Verify("Incorrect-Password", hash));
    }

    [Fact]
    public void Hash_ProducesDifferentSaltsForSamePassword()
    {
        var hash1 = _hasher.Hash("Même mot de passe");
        var hash2 = _hasher.Hash("Même mot de passe");

        Assert.NotEqual(hash1, hash2);
        // Les deux doivent pourtant être valides.
        Assert.True(_hasher.Verify("Même mot de passe", hash1));
        Assert.True(_hasher.Verify("Même mot de passe", hash2));
    }

    [Fact]
    public void Verify_ReturnsFalseForEmptyOrInvalidHash()
    {
        Assert.False(_hasher.Verify("password", string.Empty));
        Assert.False(_hasher.Verify("password", "pas-un-hash"));
        Assert.False(_hasher.Verify(string.Empty, "$PBKDF2$100000$AA==$BB=="));
    }

    [Fact]
    public void Verify_HandlesUnicodePassword()
    {
        var hash = _hasher.Hash("MotDePasse-éàç€");

        Assert.True(_hasher.Verify("MotDePasse-éàç€", hash));
    }
}