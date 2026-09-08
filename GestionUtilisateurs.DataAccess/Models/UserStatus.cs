namespace GestionUtilisateurs.DataAccess.Models;

/// <summary>
/// Statut d'un compte utilisateur.
/// </summary>
public enum UserStatus
{
    /// <summary>Compte actif, peut se connecter.</summary>
    Actif = 1,

    /// <summary>Compte inactif, connexion bloquée.</summary>
    Inactif = 2
}
