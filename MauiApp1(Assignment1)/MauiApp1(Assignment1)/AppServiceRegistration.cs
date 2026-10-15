using MauiApp1_Assignment1_.Services;
using MauiApp1_Assignment1_.ViewModels;
using MauiApp1_Assignment1_.Views;
using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1_Assignment1_;

// Registers everything the app needs. MauiProgram.cs calls it with: builder.AddAppServices();
public static class AppServiceRegistration
{
    public static MauiAppBuilder AddAppServices(this MauiAppBuilder builder)
    {
        // One HttpClient for the whole app, pointing at the Web API
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(ApiSettings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(15)
        });

        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<CustomerService>();
        builder.Services.AddSingleton<ActionMessageService>();

        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<CustomerListViewModel>();
        builder.Services.AddTransient<CustomerEditViewModel>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<CustomerListPage>();
        builder.Services.AddTransient<CustomerEditPage>();

        return builder;
    }
}
