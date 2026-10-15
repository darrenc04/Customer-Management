namespace MauiApp1_Assignment1_;

// One place for the page names used in navigation
public static class Routes
{
    public const string Login = "LoginPage";
    public const string CustomerList = "CustomerListPage";
    public const string CustomerEdit = "CustomerEditPage";

    // "//" means "go to this page and clear the back stack"
    public const string LoginAbsolute = "//" + Login;
    public const string CustomerListAbsolute = "//" + CustomerList;
}
