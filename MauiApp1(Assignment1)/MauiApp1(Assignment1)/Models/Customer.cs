namespace MauiApp1_Assignment1_.Models;

public class Customer
{
    public int CustomerId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Contact { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastUpdate { get; set; }

    //Text Helper for list page
    public string AgeText => $"Age: {Age}";
    public string CreatedText => $"Created {CreatedDate:yyyy-MM-dd HH:mm} by {CreatedBy}";
    public string LastUpdateText => LastUpdate.HasValue ? $"Last Updated: {LastUpdate:yyyy-MM-dd HH:mm}" : "Never Update";
}
