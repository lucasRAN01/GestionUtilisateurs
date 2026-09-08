using GestionUtilisateurs.DataAccess.Services;
using GestionUtilisateurs.Services;

namespace GestionUtilisateurs.ViewModels;

public class AdminDashboardViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    private int _totalUtilisateurs;
    private int _utilisateursActifs;
    private int _utilisateursInactifs;

    public AdminDashboardViewModel(
        IUserService userService,
        IAuthService authService,
        INavigationService navigationService)
    {
        _userService = userService;
        _authService = authService;
        _navigationService = navigationService;

        Title = "Tableau de bord";

        RefreshCommand = new AsyncRelayCommand(LoadDataAsync);
        NavigateUsersCommand = new AsyncRelayCommand(async () => await _navigationService.GoToUserListAsync());
        NavigateAuditCommand = new AsyncRelayCommand(async () => await _navigationService.GoToAuditLogAsync());
        NavigateChangePasswordCommand = new AsyncRelayCommand(async () => await _navigationService.GoToChangePasswordAsync());
        LogoutCommand = new AsyncRelayCommand(async () =>
        {
            try
            {
                await _authService.LogoutAsync();
                await _navigationService.GoToLoginAsync();
            }
            catch (Exception ex)
            {
                ShowError($"Erreur lors de la déconnexion : {ex.Message}");
            }
        });
    }

    public int TotalUtilisateurs
    {
        get => _totalUtilisateurs;
        private set => SetProperty(ref _totalUtilisateurs, value);
    }

    public int UtilisateursActifs
    {
        get => _utilisateursActifs;
        private set => SetProperty(ref _utilisateursActifs, value);
    }

    public int UtilisateursInactifs
    {
        get => _utilisateursInactifs;
        private set => SetProperty(ref _utilisateursInactifs, value);
    }

    public string NomUtilisateurCourant => _authService.CurrentUsername ?? string.Empty;

    public string RoleCourant => _authService.CurrentRole ?? string.Empty;

    public AsyncRelayCommand RefreshCommand { get; }
    public AsyncRelayCommand NavigateUsersCommand { get; }
    public AsyncRelayCommand NavigateAuditCommand { get; }
    public AsyncRelayCommand NavigateChangePasswordCommand { get; }
    public AsyncRelayCommand LogoutCommand { get; }

    public async Task OnAppearingAsync()
    {
        if (RefreshCommand.CanExecute(null))
        {
            await LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ClearMessages();

        try
        {
            var users = await _userService.GetAllAsync();
            TotalUtilisateurs = users.Count;
            UtilisateursActifs = users.Count(u => u.Status == DataAccess.Models.UserStatus.Actif);
            UtilisateursInactifs = users.Count(u => u.Status == DataAccess.Models.UserStatus.Inactif);
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des données : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
