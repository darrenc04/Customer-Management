using MauiApp1_Assignment1_.ViewModels;

namespace MauiApp1_Assignment1_.Views;

public partial class CustomerListPage : ContentPage
{
    private readonly CustomerListViewModel _viewModel;

    public CustomerListPage(CustomerListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    // Runs every time the page is shown, so the list is fresh after adding or editing
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.ShowPendingMessage();
        await _viewModel.LoadAsync();
    }
}
