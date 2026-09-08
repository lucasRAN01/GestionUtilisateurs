using GestionUtilisateurs.ViewModels;

namespace GestionUtilisateurs.Views;

[QueryProperty(nameof(UserIdParam), "userId")]
public partial class UserDetailPage : ContentPage
{
    private readonly UserDetailViewModel _viewModel;

    public UserDetailPage(UserDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    /// <summary>Paramètre reçu lors de la navigation (0 = création).</summary>
    public string UserIdParam
    {
        set => _userId = int.TryParse(value, out var id) ? id : 0;
    }

    private int _userId;

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync(_userId);
    }
}