namespace MauiApp1_Assignment1_.Services;

public class ApiResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public bool IsUnauthorized { get; set; }
}
public class ApiResult<T> : ApiResult
{
    public T? Data { get; set; }
}
