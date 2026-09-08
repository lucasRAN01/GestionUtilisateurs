using GestionUtilisateurs.ViewModels;

namespace GestionUtilisateurs.Views;

public partial class SetupAdminPage : ContentPage
{
    private readonly SetupAdminViewModel _viewModel;

    public SetupAdminPage(SetupAdminViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearingAsync();
    }
}