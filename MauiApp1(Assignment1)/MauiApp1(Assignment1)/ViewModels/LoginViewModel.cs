using MauiApp1_Assignment1_.Models;
using MauiApp1_Assignment1_.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiApp1_Assignment1_.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;

    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _errorMessage = string.Empty;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        LoginCommand = new Command(async () => await LoginAsync());
    }

    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set
        {
            if (SetProperty(ref _errorMessage, value))
                OnPropertyChanged(nameof(HasError));
        }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public ICommand LoginCommand { get; }

    //Called when the login page appears: skip the login if a saved token is still valid
    public async Task InitializeAsync()
    {
        if (await _authService.TryRestoreSessionAsync())
            await Shell.Current.GoToAsync(Routes.CustomerListAbsolute);
    }

    private async Task LoginAsync()
    {
        if (IsBusy)
            return;

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        IsBusy = true;
        try
        {
            ApiResult<LoginResponse> result = await _authService.LoginAsync(Username.Trim(), Password);
            if (!result.Success)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            Password = string.Empty;
            await Shell.Current.GoToAsync(Routes.CustomerListAbsolute);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
