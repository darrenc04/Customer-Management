namespace CustomerApi.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string Username { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Contact { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}
