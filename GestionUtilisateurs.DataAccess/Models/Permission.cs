namespace GestionUtilisateurs.DataAccess.Models;

/// <summary>
/// Représente une permission configurable accordée à un rôle.
/// </summary>
public class Permission
{
    /// <summary>Identifiant unique de la permission.</summary>
    public int Id { get; set; }

    /// <summary>Clé de la permission (ex: "users.create", "users.delete").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Libellé lisible de la permission.</summary>
    public string Libelle { get; set; } = string.Empty;

    /// <summary>Description de la permission.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Rôles qui disposent de cette permission.</summary>
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
