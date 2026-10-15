using MauiApp1_Assignment1_.Views;

namespace MauiApp1_Assignment1_;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        // The edit page is opened on top of the list, so it is registered as a route
        Routing.RegisterRoute(Routes.CustomerEdit, typeof(CustomerEditPage));
    }
}
