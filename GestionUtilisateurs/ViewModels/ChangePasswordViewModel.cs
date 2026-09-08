using GestionUtilisateurs.DataAccess.Services;
using GestionUtilisateurs.Services;

namespace GestionUtilisateurs.ViewModels;

public class ChangePasswordViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmNewPassword = string.Empty;

    public ChangePasswordViewModel(
        IUserService userService,
        IAuthService authService,
        INavigationService navigationService)
    {
        _userService = userService;
        _authService = authService;
        _navigationService = navigationService;

        Title = "Changer le mot de passe";

        SaveCommand = new AsyncRelayCommand(SaveAsync);
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
    }

    public AsyncRelayCommand SaveCommand { get; }

    public Task OnAppearingAsync()
    {
        ClearMessages();
        return Task.CompletedTask;
    }

    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        ClearMessages();

        try
        {
            if (_authService.CurrentUserId is not int currentUserId)
            {
                ShowError("Aucun utilisateur connecté.");
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentPassword))
            {
                ShowError("Veuillez saisir votre mot de passe actuel.");
                return;
            }

            if (NewPassword.Length < 6)
            {
                ShowError("Le nouveau mot de passe doit contenir au moins 6 caractères.");
                return;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                ShowError("La confirmation du mot de passe ne correspond pas.");
                return;
            }

            if (NewPassword == CurrentPassword)
            {
                ShowError("Le nouveau mot de passe doit être différent du mot de passe actuel.");
                return;
            }

            var username = _authService.CurrentUsername ?? string.Empty;
            var loginResult = await _authService.LoginAsync(username, CurrentPassword);
            if (!loginResult.Success)
            {
                ShowError("Le mot de passe actuel est incorrect.");
                return;
            }

            var result = await _userService.ResetPasswordAsync(currentUserId, NewPassword, currentUserId);
            if (result.Success)
            {
                ShowSuccess("Mot de passe modifié avec succès.");
                ClearFields();
                await _navigationService.GoToDashboardAsync();
            }
            else
            {
                ShowError(result.Message ?? "Échec de la modification du mot de passe.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors de la modification du mot de passe : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ClearFields()
    {
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmNewPassword = string.Empty;
    }
}
