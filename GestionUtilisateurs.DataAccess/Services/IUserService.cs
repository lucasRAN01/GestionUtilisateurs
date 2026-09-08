using GestionUtilisateurs.DataAccess.Models;

namespace GestionUtilisateurs.DataAccess.Services;

/// <summary>
/// Résultat des opérations de gestion des utilisateurs.
/// </summary>
public class UserOperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

/// <summary>
/// Interface du service de gestion des utilisateurs (CRUD, rôles, réinitialisation).
/// </summary>
public interface IUserService
{
    /// <summary>Récupère tous les utilisateurs avec leur rôle.</summary>
    Task<IReadOnlyList<User>> GetAllAsync();

    /// <summary>Récupère un utilisateur par identifiant.</summary>
    Task<User?> GetByIdAsync(int id);

    /// <summary>Crée un nouvel utilisateur.</summary>
    Task<UserOperationResult> CreateAsync(User user, string password, int actorUserId);

    /// <summary>Met à jour les informations d'un utilisateur.</summary>
    Task<UserOperationResult> UpdateAsync(User user, int actorUserId);

    /// <summary>Supprime un utilisateur.</summary>
    Task<UserOperationResult> DeleteAsync(int id, int actorUserId);

    /// <summary>Bascule l'activation/désactivation d'un utilisateur.</summary>
    Task<UserOperationResult> ToggleStatusAsync(int id, int actorUserId);

    /// <summary>Réinitialise le mot de passe d'un utilisateur.</summary>
    Task<UserOperationResult> ResetPasswordAsync(int id, string newPassword, int actorUserId);

    /// <summary>Attribue un rôle à un utilisateur.</summary>
    Task<UserOperationResult> AssignRoleAsync(int userId, int roleId, int actorUserId);

    /// <summary>Vérifie l'unicité du nom d'utilisateur et de l'e-mail.</summary>
    Task<bool> IsUsernameTakenAsync(string username, int? excludeId = null);

    /// <summary>Vérifie l'unicité de l'e-mail.</summary>
    Task<bool> IsEmailTakenAsync(string email, int? excludeId = null);
}
