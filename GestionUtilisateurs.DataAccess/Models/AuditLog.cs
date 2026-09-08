namespace GestionUtilisateurs.DataAccess.Models;

/// <summary>
/// Journal d'audit retraçant les actions importantes de l'application.
/// </summary>
public class AuditLog
{
    /// <summary>Identifiant unique de l'entrée du journal.</summary>
    public int Id { get; set; }

    /// <summary>Heure et date de l'action.</summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>Identifiant de l'utilisateur qui a effectué l'action.</summary>
    public int? UserId { get; set; }

    /// <summary>Utilisateur qui a effectué l'action.</summary>
    public virtual User? User { get; set; }

    /// <summary>Type d'action journalisée.</summary>
    public AuditActionType ActionType { get; set; }

    /// <summary>Libellé décrivant l'action.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Détails additionnels (JSON ou texte libre).</summary>
    public string? Details { get; set; }

    /// <summary>Nom d'utilisateur impliqué (si différent de l'acteur).</summary>
    public string? TargetUsername { get; set; }
}
