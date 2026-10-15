using MauiApp1_Assignment1_.Models;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace MauiApp1_Assignment1_.Services;

// Logs in, keeps the token, and remembers it securely on the device
public class AuthService
{
    private const string TokenKey = "auth_token";
    private const string ExpiresKey = "auth_expires";
    private const string UserKey = "auth_user";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string? Token { get; private set; }
    public string? Username { get; private set; }

    public async Task<ApiResult<LoginResponse>> LoginAsync(string username, string password)
    {
        try
        {
            var request = new LoginRequest { Username = username, Password = password };
            using HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/login", request, JsonOptions);
            
            if (!response.IsSuccessStatusCode)
            {
                string message = response.StatusCode == HttpStatusCode.Unauthorized
                    ? "Invalid username and password" : await ApiErrorReader.ReadAsync(response);

                return new ApiResult<LoginResponse>
                {
                    ErrorMessage = message
                };
            }
            LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions);
            if(login is null || string.IsNullOrEmpty(login.Token))
            {
                return new ApiResult<LoginResponse> { ErrorMessage = "The server sent an unexpected answer." };
            }

            Token = login.Token;
            Username = login.Username;
            await SaveSessionAsync(login);

            return new ApiResult<LoginResponse> { Success = true, Data = login };
            
        }
        catch (HttpRequestException)
        {
            return new ApiResult<LoginResponse> { ErrorMessage = ApiErrorReader.NetworkMessage };
        }
        catch (TaskCanceledException)
        {
            return new ApiResult<LoginResponse> { ErrorMessage = ApiErrorReader.TimeoutMessage };
        }
    }

    // Used when the app starts: if a saved token is still valid, skip the login page
    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            string? token = await SecureStorage.Default.GetAsync(TokenKey);
            string? expies = await SecureStorage.Default.GetAsync(ExpiresKey);
            string? username = await SecureStorage.Default.GetAsync(UserKey);

            if(string.IsNullOrEmpty(token) || !DateTime.TryParse(ExpiresKey, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime expiresAt))
            {
                return false;
            }

            if(expiresAt.ToUniversalTime() <= DateTime.UtcNow.AddMinutes(1))
            {
                Logout();
                return false;
            }
            Token = token;
            Username = username;
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void Logout()
    {
        Token = null;
        Username = null;
        
        try
        {
            SecureStorage.Default.Remove(TokenKey);
            SecureStorage.Default.Remove(ExpiresKey);
            SecureStorage.Default.Remove(UserKey);
        }
        catch(Exception)
        {

        }
    }

    private static async Task SaveSessionAsync(LoginResponse login)
    {
        try
        {
            await SecureStorage.Default.SetAsync(TokenKey, login.Token);
            await SecureStorage.Default.SetAsync(ExpiresKey, login.ExpireAt.ToString("o", CultureInfo.InvariantCulture));
            await SecureStorage.Default.SetAsync(UserKey, login.Username);
        }
        catch (Exception)
        {
            // The token still works in memory for this session
        }
    }
}
