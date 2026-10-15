using MauiApp1_Assignment1_.ViewModels;

namespace MauiApp1_Assignment1_.Views;

public partial class CustomerEditPage : ContentPage
{
    public CustomerEditPage(CustomerEditViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
