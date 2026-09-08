using GestionUtilisateurs.DataAccess.Models;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Résultat d'une tentative de connexion.
/// </summary>
public class LoginResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public User? User { get; set; }
    public bool IsLockedOut { get; set; }
    public int RemainingAttempts { get; set; }
}

/// <summary>
/// Interface du service d'authentification locale.
/// </summary>
public interface IAuthService
{
    /// <summary>Identifiant de l'utilisateur connecté dans la session courante.</summary>
    int? CurrentUserId { get; }

    /// <summary>Nom d'utilisateur connecté dans la session courante.</summary>
    string? CurrentUsername { get; }

    /// <summary>Rôle de l'utilisateur connecté.</summary>
    string? CurrentRole { get; }

    /// <summary>Indique si un utilisateur est connecté.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Nombre maximal d'échecs avant verrouillage.</summary>
    int MaxFailedAttempts { get; }

    /// <summary>Durée de verrouillage (secondes).</summary>
    int LockoutSeconds { get; }

    /// <summary>Tente de connecter un utilisateur.</summary>
    Task<LoginResult> LoginAsync(string username, string password);

    /// <summary>Déconnecte l'utilisateur courant.</summary>
    Task LogoutAsync();

    /// <summary>Récupère l'utilisateur connecté complet.</summary>
    Task<User?> GetCurrentUserAsync();
}
