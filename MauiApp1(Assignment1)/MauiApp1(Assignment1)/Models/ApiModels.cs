namespace MauiApp1_Assignment1_.Models;

//send to POST api/auth/login
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

//what the API answers to a successful login
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime ExpireAt { get; set; }
}

//what the app send when adding or editing customer
public class CustomerRequest
{
    public string Username { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}