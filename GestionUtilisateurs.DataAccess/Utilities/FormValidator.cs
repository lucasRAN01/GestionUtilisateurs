using System.Text.RegularExpressions;

namespace GestionUtilisateurs.DataAccess.Utilities;

/// <summary>
/// Utilitaires de validation de formulaires pour les formulaires de saisie.
/// </summary>
public static class FormValidator
{
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"^[\+]?[\d\s\-()]{7,20}$", RegexOptions.Compiled);

    public static string? ValidateRequired(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return $"Le champ « {fieldName} » est obligatoire.";
        return null;
    }

    public static string? ValidateEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (!EmailRegex.IsMatch(value))
            return "L'adresse e-mail n'est pas valide.";
        return null;
    }

    public static string? ValidatePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        if (!PhoneRegex.IsMatch(value))
            return "Le numéro de téléphone n'est pas valide.";
        return null;
    }

    public static string? ValidatePassword(string? value, int minLength = 6)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Le mot de passe est obligatoire.";
        if (value!.Length < minLength)
            return $"Le mot de passe doit contenir au moins {minLength} caractères.";
        if (value.Length > 128)
            return "Le mot de passe ne peut pas dépasser 128 caractères.";
        return null;
    }

    public static string? ValidatePasswordMatch(string? password, string? confirm)
    {
        if (password != confirm)
            return "Les mots de passe ne correspondent pas.";
        return null;
    }

    public static string? ValidateUsername(string? value)
    {
        var required = ValidateRequired(value, "Nom d'utilisateur");
        if (required != null) return required;
        if (value!.Length < 3)
            return "Le nom d'utilisateur doit contenir au moins 3 caractères.";
        if (value.Length > 100)
            return "Le nom d'utilisateur ne peut pas dépasser 100 caractères.";
        return null;
    }
}
