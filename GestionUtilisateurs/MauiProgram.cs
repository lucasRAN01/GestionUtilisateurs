using GestionUtilisateurs.DataAccess.Data;
using GestionUtilisateurs.DataAccess.Services;
using GestionUtilisateurs.Services;
using GestionUtilisateurs.ViewModels;
using GestionUtilisateurs.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;

namespace GestionUtilisateurs;

/// <summary>
/// Point d'entrée de l'application MAUI.
/// Configure les services, la base de données et la navigation.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>();

        // ---------- Logging ----------
        builder.Logging.AddDebug();

        // ---------- Chemin de la base SQLite ----------
        var databasePath = Path.Combine(FileSystem.AppDataDirectory, "GestionUtilisateurs.db");

        // ---------- Services DataAccess ----------
        builder.Services.AddSingleton<IAppDbFactory>(new AppDbFactory(databasePath));
        builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        builder.Services.AddSingleton<IDatabaseInitializer, DatabaseInitializer>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<IUserService, UserService>();
        builder.Services.AddSingleton<IRoleService, RoleService>();
        builder.Services.AddSingleton<IAuditLogger, AuditLogger>();
        builder.Services.AddSingleton<IAuditService, AuditService>();

        // ---------- Services applicatifs ----------
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        // ---------- ViewModels ----------
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<AdminDashboardViewModel>();
        builder.Services.AddTransient<UserListViewModel>();
        builder.Services.AddTransient<UserDetailViewModel>();
        builder.Services.AddTransient<AuditLogViewModel>();
        builder.Services.AddTransient<SetupAdminViewModel>();
        builder.Services.AddTransient<ChangePasswordViewModel>();

        // ---------- Pages ----------
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<SetupAdminPage>();
        builder.Services.AddTransient<AdminDashboardPage>();
        builder.Services.AddTransient<UserListPage>();
        builder.Services.AddTransient<UserDetailPage>();
        builder.Services.AddTransient<AuditLogPage>();
        builder.Services.AddTransient<ChangePasswordPage>();

        return builder.Build();
    }
}
