using System.Collections.ObjectModel;
using GestionUtilisateurs.DataAccess.Models;
using GestionUtilisateurs.DataAccess.Services;
using GestionUtilisateurs.Services;

namespace GestionUtilisateurs.ViewModels;

public class UserDetailViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;

    public UserDetailViewModel(
        IUserService userService,
        IRoleService roleService,
        IAuthService authService,
        INavigationService navigationService)
    {
        _userService = userService;
        _roleService = roleService;
        _authService = authService;
        _navigationService = navigationService;

        SaveCommand = new AsyncRelayCommand(ExecuteSaveAsync);
        ChangePasswordCommand = new AsyncRelayCommand(ExecuteChangePasswordAsync);
        ToggleStatusCommand = new AsyncRelayCommand(ExecuteToggleStatusAsync);
    }

    public int UserId { get; private set; }
    public bool IsEditMode => UserId > 0;
    public bool IsCreateMode => !IsEditMode;

    private string _nom = string.Empty;
    public string Nom
    {
        get => _nom;
        set => SetProperty(ref _nom, value);
    }

    private string _prenom = string.Empty;
    public string Prenom
    {
        get => _prenom;
        set => SetProperty(ref _prenom, value);
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string? _telephone;
    public string? Telephone
    {
        get => _telephone;
        set => SetProperty(ref _telephone, value);
    }

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    private string _confirmNewPassword = string.Empty;
    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
    }

    private UserStatus _status = UserStatus.Actif;
    public UserStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
                OnPropertyChanged(nameof(StatusLabel));
        }
    }

    public string StatusLabel => Status == UserStatus.Actif ? "Actif" : "Inactif";

    private Role? _selectedRole;
    public Role? SelectedRole
    {
        get => _selectedRole;
        set => SetProperty(ref _selectedRole, value);
    }

    public ObservableCollection<Role> RoleList { get; } = new();

    public AsyncRelayCommand SaveCommand { get; }
    public AsyncRelayCommand ChangePasswordCommand { get; }
    public AsyncRelayCommand ToggleStatusCommand { get; }

    public async Task InitializeAsync(int userId)
    {
        UserId = userId;
        OnPropertyChanged(nameof(IsEditMode));
        OnPropertyChanged(nameof(IsCreateMode));

        Title = IsEditMode ? "Modifier l'utilisateur" : "Nouvel utilisateur";

        ClearMessages();
        RoleList.Clear();
        SelectedRole = null;

        try
        {
            var roles = await _roleService.GetAllAsync();
            foreach (var role in roles)
                RoleList.Add(role);

            if (IsEditMode)
            {
                var user = await _userService.GetByIdAsync(userId);
                if (user is null)
                {
                    ShowError("Utilisateur introuvable.");
                    return;
                }

                Nom = user.Nom;
                Prenom = user.Prenom;
                Email = user.Email;
                Telephone = user.Telephone;
                Username = user.Username;
                Notes = user.Notes;
                Status = user.Status;

                SelectedRole = RoleList.FirstOrDefault(r => r.Id == user.RoleId);
            }
            else
            {
                Nom = string.Empty;
                Prenom = string.Empty;
                Email = string.Empty;
                Telephone = null;
                Username = string.Empty;
                Notes = null;
                Status = UserStatus.Actif;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;

                SelectedRole = RoleList.FirstOrDefault();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement : {ex.Message}");
        }
    }

    private async Task ExecuteSaveAsync()
    {
        if (IsBusy) return;

        ClearMessages();

        if (!Validate()) return;

        IsBusy = true;

        try
        {
            if (IsEditMode)
            {
                var user = await _userService.GetByIdAsync(UserId);
                if (user is null)
                {
                    ShowError("Utilisateur introuvable.");
                    return;
                }

                user.Nom = Nom.Trim();
                user.Prenom = Prenom.Trim();
                user.Email = Email.Trim();
                user.Telephone = string.IsNullOrWhiteSpace(Telephone) ? null : Telephone.Trim();
                user.Username = Username.Trim();
                user.Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim();
                user.Status = Status;

                var updateResult = await _userService.UpdateAsync(user, _authService.CurrentUserId ?? 0);
                if (!updateResult.Success)
                {
                    ShowError(updateResult.Message ?? "Erreur lors de la mise à jour.");
                    return;
                }

                if (SelectedRole is not null && SelectedRole.Id != user.RoleId)
                {
                    var roleResult = await _userService.AssignRoleAsync(UserId, SelectedRole.Id, _authService.CurrentUserId ?? 0);
                    if (!roleResult.Success)
                    {
                        ShowError(roleResult.Message ?? "Erreur lors de l'assignation du rôle.");
                        return;
                    }
                }

                ShowSuccess("Utilisateur mis à jour avec succès.");
            }
            else
            {
                var usernameTaken = await _userService.IsUsernameTakenAsync(Username.Trim());
                if (usernameTaken)
                {
                    ShowError("Ce nom d'utilisateur est déjà pris.");
                    return;
                }

                var emailTaken = await _userService.IsEmailTakenAsync(Email.Trim());
                if (emailTaken)
                {
                    ShowError("Cette adresse e-mail est déjà utilisée.");
                    return;
                }

                var newUser = new User
                {
                    Nom = Nom.Trim(),
                    Prenom = Prenom.Trim(),
                    Email = Email.Trim(),
                    Telephone = string.IsNullOrWhiteSpace(Telephone) ? null : Telephone.Trim(),
                    Username = Username.Trim(),
                    Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
                    RoleId = SelectedRole?.Id ?? 0,
                    Status = Status
                };

                var createResult = await _userService.CreateAsync(newUser, NewPassword, _authService.CurrentUserId ?? 0);
                if (!createResult.Success)
                {
                    ShowError(createResult.Message ?? "Erreur lors de la création.");
                    return;
                }

                ShowSuccess("Utilisateur créé avec succès.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExecuteChangePasswordAsync()
    {
        await _navigationService.GoToChangePasswordAsync();
    }

    private async Task ExecuteToggleStatusAsync()
    {
        if (!IsEditMode || IsBusy) return;

        IsBusy = true;
        ClearMessages();

        try
        {
            var actorId = _authService.CurrentUserId ?? 0;
            var result = await _userService.ToggleStatusAsync(UserId, actorId);

            if (result.Success)
                await InitializeAsync(UserId);
            else
                ShowError(result.Message ?? "Erreur lors du changement de statut.");
        }
        catch (Exception ex)
        {
            ShowError($"Erreur : {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(Nom))
        {
            ShowError("Le nom est requis.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Prenom))
        {
            ShowError("Le prénom est requis.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ShowError("L'e-mail est requis.");
            return false;
        }

        if (!Email.Contains('@') || !Email.Contains('.'))
        {
            ShowError("L'adresse e-mail n'est pas valide.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            ShowError("Le nom d'utilisateur est requis.");
            return false;
        }

        if (SelectedRole is null)
        {
            ShowError("Veuillez sélectionner un rôle.");
            return false;
        }

        if (IsCreateMode)
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ShowError("Le mot de passe est requis.");
                return false;
            }

            if (NewPassword.Length < 6)
            {
                ShowError("Le mot de passe doit contenir au moins 6 caractères.");
                return false;
            }

            if (NewPassword != ConfirmNewPassword)
            {
                ShowError("Les mots de passe ne correspondent pas.");
                return false;
            }
        }

        return true;
    }
}
