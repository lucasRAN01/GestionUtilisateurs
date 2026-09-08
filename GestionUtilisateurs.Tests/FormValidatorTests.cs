using GestionUtilisateurs.DataAccess.Utilities;
using Xunit;

namespace GestionUtilisateurs.Tests;

public class FormValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateRequired_EmptyValues_ReturnError(string? value)
    {
        var error = FormValidator.ValidateRequired(value, "Champ");

        Assert.NotNull(error);
        Assert.Contains("Champ", error);
    }

    [Fact]
    public void ValidateRequired_ValidValue_ReturnsNull()
    {
        Assert.Null(FormValidator.ValidateRequired("valeur", "Champ"));
    }

    [Theory]
    [InlineData("toto@domaine.fr")]
    [InlineData("jean.pierre@exemple.com")]
    [InlineData("a@b.co")]
    public void ValidateEmail_ValidEmails_ReturnNull(string email)
    {
        Assert.Null(FormValidator.ValidateEmail(email));
    }

    [Theory]
    [InlineData("toto")]
    [InlineData("toto@")]
    [InlineData("@domaine")]
    [InlineData("toto@domaine")]
    public void ValidateEmail_InvalidEmails_ReturnError(string email)
    {
        Assert.NotNull(FormValidator.ValidateEmail(email));
    }

    [Fact]
    public void ValidateEmail_Empty_ReturnsNull()
    {
        Assert.Null(FormValidator.ValidateEmail(string.Empty));
    }

    [Theory]
    [InlineData("123456", false)]
    [InlineData("1234567890", false)]
    [InlineData("abcdef", false)]
    [InlineData("abc", true)]
    [InlineData("123", true)]
    [InlineData("12345", true)]
    public void ValidatePassword_DifferentLengths(string password, bool isInvalid)
    {
        var error = FormValidator.ValidatePassword(password, minLength: 6);

        if (isInvalid)
            Assert.NotNull(error);
        else
            Assert.Null(error);
    }

    [Fact]
    public void ValidatePasswordMatch_Mismatch_ReturnsError()
    {
        Assert.NotNull(FormValidator.ValidatePasswordMatch("abc", "abd"));
    }

    [Fact]
    public void ValidatePasswordMatch_Match_ReturnsNull()
    {
        Assert.Null(FormValidator.ValidatePasswordMatch("abc", "abc"));
    }

    [Theory]
    [InlineData("ab", true)]
    [InlineData("abc", false)]
    [InlineData("", true)]
    [InlineData(null, true)]
    public void ValidateUsername_Length(string username, bool isInvalid)
    {
        var error = FormValidator.ValidateUsername(username);

        if (isInvalid)
            Assert.NotNull(error);
        else
            Assert.Null(error);
    }

    [Theory]
    [InlineData("0123456789")]
    [InlineData("+33 6 12 34 56 78")]
    [InlineData("+261 34 00 000 00")]
    public void ValidatePhone_ValidPhones_ReturnNull(string phone)
    {
        Assert.Null(FormValidator.ValidatePhone(phone));
    }

    [Fact]
    public void ValidatePhone_Invalid_ReturnsError()
    {
        Assert.NotNull(FormValidator.ValidatePhone("ab!12"));
    }

    [Fact]
    public void ValidatePhone_Empty_ReturnsNull()
    {
        Assert.Null(FormValidator.ValidatePhone(string.Empty));
    }
}