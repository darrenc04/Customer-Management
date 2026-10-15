namespace CustomerApi.Exceptions
{
    public class DuplicateUsernameException : Exception
    {
        public DuplicateUsernameException() : base("Username already exists."){ }
    }

    public class CustomerNotFoundException : Exception{
        public CustomerNotFoundException() : base("Customer not found."){ }
    }
}
