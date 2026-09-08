using GestionUtilisateurs.Views;

namespace GestionUtilisateurs.Services;

/// <summary>
/// Interface de navigation centralisée.
/// Permet de naviguer vers les pages sans référencer directement les vues.
/// </summary>
public interface INavigationService
{
    /// <summary>Navigue vers la page de connexion (réinitialise la pile).</summary>
    Task GoToLoginAsync();

    /// <summary>Navigue vers la page de configuration du premier administrateur.</summary>
    Task GoToSetupAdminAsync();

    /// <summary>Navigue vers le tableau de bord administrateur.</summary>
    Task GoToDashboardAsync();

    /// <summary>Navigue vers la liste des utilisateurs.</summary>
    Task GoToUserListAsync();

    /// <summary>Navigue vers la fiche d'un utilisateur.</summary>
    Task GoToUserDetailAsync(int userId = 0);

    /// <summary>Navigue vers le journal d'audit.</summary>
    Task GoToAuditLogAsync();

    /// <summary>Navigue vers la page de changement de mot de passe.</summary>
    Task GoToChangePasswordAsync();
}

/// <summary>
/// Implémentation de la navigation centralisée via Shell.
/// </summary>
public class NavigationService : INavigationService
{
    public Task GoToLoginAsync() =>
        Shell.Current.GoToAsync("//LoginPage");

    public Task GoToSetupAdminAsync() =>
        Shell.Current.GoToAsync("//SetupAdminPage");

    public Task GoToDashboardAsync() =>
        Shell.Current.GoToAsync("//AdminDashboard");

    public Task GoToUserListAsync() =>
        Shell.Current.GoToAsync("//UserList");

    public Task GoToUserDetailAsync(int userId = 0) =>
        Shell.Current.GoToAsync($"//UserDetail?userId={userId}");

    public Task GoToAuditLogAsync() =>
        Shell.Current.GoToAsync("//AuditLog");

    public Task GoToChangePasswordAsync() =>
        Shell.Current.GoToAsync("//ChangePassword");
}
