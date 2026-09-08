namespace GestionUtilisateurs.DataAccess.Models;

/// <summary>
/// Un rôle définit un ensemble de permissions accordées à un utilisateur.
/// </summary>
public class Role
{
    /// <summary>Identifiant unique du rôle.</summary>
    public int Id { get; set; }

    /// <summary>Nom du rôle (Administrateur, Gestionnaire, Utilisateur...).</summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>Description du rôle.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Date et heure de création du rôle.</summary>
    public DateTime DateCreation { get; set; } = DateTime.Now;

    /// <summary>Utilisateurs associés à ce rôle.</summary>
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>Permissions accordées à ce rôle.</summary>
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
