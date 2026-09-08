namespace GestionUtilisateurs.DataAccess.Models;

/// <summary>
/// Enumération des rôles disponibles dans l'application.
/// </summary>
public enum RoleType
{
    /// <summary>Accès complet à toutes les fonctionnalités.</summary>
    Administrateur = 1,

    /// <summary>Gère les utilisateurs sans droits système avancés.</summary>
    Gestionnaire = 2,

    /// <summary>Accès en lecture seule.</summary>
    Utilisateur = 3
}
