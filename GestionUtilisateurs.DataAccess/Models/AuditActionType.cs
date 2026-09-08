namespace GestionUtilisateurs.DataAccess.Models;

/// <summary>
/// Type d'action journalisée.
/// </summary>
public enum AuditActionType
{
    /// <summary>Connexion réussie.</summary>
    Connexion = 1,

    /// <summary>Tentative de connexion échouée.</summary>
    EchecConnexion = 2,

    /// <summary>Déconnexion de l'utilisateur.</summary>
    Deconnexion = 3,

    /// <summary>Création d'un utilisateur.</summary>
    Creation = 4,

    /// <summary>Modification d'un utilisateur.</summary>
    Modification = 5,

    /// <summary>Suppression d'un utilisateur.</summary>
    Suppression = 6,

    /// <summary>Activation ou désactivation d'un compte.</summary>
    Activation = 7,

    /// <summary>Réinitialisation du mot de passe.</summary>
    ResetMotDePasse = 8,

    /// <summary>Attribution d'un rôle.</summary>
    AttributionRole = 9
}
