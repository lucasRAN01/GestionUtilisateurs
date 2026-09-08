namespace GestionUtilisateurs.DataAccess.Models;

/// <summary>
/// Utilisateur de l'application.
/// </summary>
public class User
{
    /// <summary>Identifiant unique de l'utilisateur.</summary>
    public int Id { get; set; }

    /// <summary>Nom de l'utilisateur.</summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>Prénom de l'utilisateur.</summary>
    public string Prenom { get; set; } = string.Empty;

    /// <summary>Adresse e-mail de l'utilisateur.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Numéro de téléphone de l'utilisateur.</summary>
    public string? Telephone { get; set; }

    /// <summary>Nom d'utilisateur (identifiant de connexion).</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>Hachage PBKDF2 du mot de passe (stocké de manière sécurisée).</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>Identifiant du rôle assigné à l'utilisateur.</summary>
    public int RoleId { get; set; }

    /// <summary>Rôle de l'utilisateur.</summary>
    public virtual Role? Role { get; set; }

    /// <summary>Statut du compte (actif/inactif).</summary>
    public UserStatus Status { get; set; } = UserStatus.Actif;

    /// <summary>Date de création du compte.</summary>
    public DateTime DateCreation { get; set; } = DateTime.Now;

    /// <summary>Date de la dernière connexion réussie.</summary>
    public DateTime? DerniereConnexion { get; set; }

    /// <summary>Notes libres sur l'utilisateur.</summary>
    public string? Notes { get; set; }

    /// <summary>Nombre d'échecs de connexion consécutifs.</summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>Date de verrouillage du compte (null si non verrouillé).</summary>
    public DateTime? LockoutEnd { get; set; }

    /// <summary>Indique si le compte est verrouillé à l'instant T.</summary>
    public bool IsLocked => LockoutEnd.HasValue && LockoutEnd.Value > DateTime.Now;

    /// <summary>Journal d'audit lié à cet utilisateur.</summary>
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
