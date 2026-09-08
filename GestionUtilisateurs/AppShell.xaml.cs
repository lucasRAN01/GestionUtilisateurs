using GestionUtilisateurs.Views;

namespace GestionUtilisateurs;

/// <summary>
/// Shell principal de l'application.
/// Mappage des routes pour la navigation globale.
/// </summary>
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Enregistrement des routes pour la navigation (pages non liées au menu).
        Routing.RegisterRoute("SetupAdminPage", typeof(SetupAdminPage));
        Routing.RegisterRoute("AdminDashboard", typeof(AdminDashboardPage));
        Routing.RegisterRoute("UserList", typeof(UserListPage));
        Routing.RegisterRoute("UserDetail", typeof(UserDetailPage));
        Routing.RegisterRoute("AuditLog", typeof(AuditLogPage));
        Routing.RegisterRoute("ChangePassword", typeof(ChangePasswordPage));
    }
}
