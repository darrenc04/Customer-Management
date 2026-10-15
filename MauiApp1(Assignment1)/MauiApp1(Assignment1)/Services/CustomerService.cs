using MauiApp1_Assignment1_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiApp1_Assignment1_.Services;

public class CustomerService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly AuthService _authService;

    public CustomerService(HttpClient httpclient, AuthService authService)
    {
        _httpClient = httpclient;
        _authService = authService;
    }

    //Empty search text returns everyone
    public Task<ApiResult<List<Customer>>> GetAllAsync(string? searchText)
    {
        string url = string.IsNullOrWhiteSpace(searchText)
            ? "api/customers"
            : $"api/customers?searchText={Uri.EscapeDataString(searchText.Trim())}";

        return SendAsync<List<Customer>>(HttpMethod.Get, url);
    }

    public Task<ApiResult<Customer>> CreateAsync(CustomerRequest request)
    {
        return SendAsync<Customer>(HttpMethod.Post, "api/customers", request);
    }

    public Task<ApiResult> UpdateAsync(int customerId, CustomerRequest request)
    {
        return SendWithoutBodyAsync(HttpMethod.Put, $"api/customers/{customerId}", request);
    }

    public Task<ApiResult> DeleteAsync(int customerId)
    {
        return SendWithoutBodyAsync(HttpMethod.Delete, $"api/customers/{customerId}");
    }

    // For calls that return data (get and post)
    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, object? body = null)
    {
        try
        {
            using HttpResponseMessage response = await SendRawAsync(method, url, body);
            if (response.IsSuccessStatusCode)
            {
                T? data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                return new ApiResult<T> { Success = true, Data = data };
            }
            return new ApiResult<T>
            {
                ErrorMessage = await ApiErrorReader.ReadAsync(response),
                IsUnauthorized = HandleUnauthorized(response)
            };
        }
        catch (HttpRequestException)
        {
            return new ApiResult<T> { ErrorMessage = ApiErrorReader.NetworkMessage };
        }
        catch (TaskCanceledException)
        {
            return new ApiResult<T> { ErrorMessage = ApiErrorReader.TimeoutMessage };
        }
    }

    //For calls that answer 204 no content (edit and delete)
    private async Task<ApiResult> SendWithoutBodyAsync(HttpMethod method, string url, object? body = null)
    {
        try
        {
            using HttpResponseMessage response = await SendRawAsync(method, url, body);

            if (response.IsSuccessStatusCode)
                return new ApiResult { Success = true };

            return new ApiResult
            {
                ErrorMessage = await ApiErrorReader.ReadAsync(response),
                IsUnauthorized = HandleUnauthorized(response)
            };
        }
        catch(HttpRequestException)
        {
            return new ApiResult { ErrorMessage = ApiErrorReader.NetworkMessage };
        }
        catch(TaskCanceledException)
        {
            return new ApiResult { ErrorMessage = ApiErrorReader.TimeoutMessage };
        }
    }

    //Builds the request and attaches the login token
    private async Task<HttpResponseMessage> SendRawAsync(HttpMethod method, string url, object? body)
    {
        using var request = new HttpRequestMessage(method, url);

        if (!string.IsNullOrEmpty(_authService.Token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.Token);

        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), null, JsonOptions);

        return await _httpClient.SendAsync(request);
    }

    //A 401 means the token is missing or expired, so forget it
    private bool HandleUnauthorized(HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return false;

        _authService.Logout();
        return true;
    }
}
