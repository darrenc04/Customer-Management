using MauiApp1_Assignment1_.Models;
using MauiApp1_Assignment1_.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiApp1_Assignment1_.ViewModels
{
    public class CustomerEditViewModel : BaseViewModel, IQueryAttributable
    {
        private static readonly Regex UsernamePattern = new(@"^[A-Za-z0-9_]+$");
        private static readonly Regex ContactPattern = new(@"^\+?[0-9]{8,11}$");
        private static readonly Regex EmailPattern = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.(?:com|com\.my)$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly CustomerService _customerService;
        private readonly ActionMessageService _actionMessageService;

        private int _customerId;

        private string _pageTitle = "Add Customer";
        private string _username = string.Empty;
        private string _ageText = string.Empty;
        private string _contact = string.Empty;
        private string _email = string.Empty;

        private string _errorMessage = string.Empty;
        private string _usernameError = string.Empty;
        private string _ageError = string.Empty;
        private string _contactError = string.Empty;
        private string _emailError = string.Empty;

        public CustomerEditViewModel(CustomerService customerService, ActionMessageService actionMessageService)
        {
            _customerService = customerService;
            _actionMessageService = actionMessageService;

            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async() => await Shell.Current.GoToAsync(".."));
        }
        
        public string PageTitle
        {
            get => _pageTitle;
            private set => SetProperty(ref _pageTitle, value);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string AgeText
        {
            get => _ageText;
            set => SetProperty(ref _ageText, value);
        }

        public string Contact
        {
            get => _contact;
            set => SetProperty(ref _contact, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        //Message from the server (for example "That username is already taken.")
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        public string UsernameError
        {
            get => _usernameError;
            private set
            {
                if (SetProperty(ref _usernameError, value))
                    OnPropertyChanged(nameof(HasUsernameError));
            }
        }
        public string AgeError
        {
            get => _ageError;
            private set
            {
                if (SetProperty(ref _ageError, value))
                    OnPropertyChanged(nameof(HasAgeError));
            }
        }

        public string ContactError
        {
            get => _contactError;
            private set
            {
                if (SetProperty(ref _contactError, value))
                    OnPropertyChanged(nameof(HasContactError));
            }
        }

        public string EmailError
        {
            get => _emailError;
            private set
            {
                if (SetProperty(ref _emailError, value))
                    OnPropertyChanged(nameof(HasEmailError));
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public bool HasUsernameError => !string.IsNullOrEmpty(UsernameError);
        public bool HasAgeError => !string.IsNullOrEmpty(AgeError);
        public bool HasContactError => !string.IsNullOrEmpty(ContactError);
        public bool HasEmailError => !string.IsNullOrEmpty(EmailError);

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("Customer", out object? value) && value is Customer customer)
            {
                _customerId = customer.CustomerId;
                PageTitle = "Edit Customer";
                Username = customer.Username;
                AgeText = customer.Age.ToString();
                Contact = customer.Contact;
                Email = customer.Email;
            }
        }
        private async Task SaveAsync()
        {
            if (IsBusy)
                return;

            _errorMessage = string.Empty;

            if (!Validate())
                return;

            IsBusy = true;
            try
            {
                var request = new CustomerRequest
                {
                    Username = Username.Trim(),
                    Age = int.Parse(AgeText.Trim()),
                    Contact = Contact.Trim(),
                    Email = Email.Trim()
                };

                bool isNew = _customerId == 0;

                ApiResult result;
                if (isNew)
                    result = await _customerService.CreateAsync(request);
                else
                    result = await _customerService.UpdateAsync(_customerId, request);

                if (result.IsUnauthorized)
                {
                    await Shell.Current.GoToAsync(Routes.LoginAbsolute);
                    return;
                }

                if (!result.Success)
                {
                    //Error message
                    await Shell.Current.DisplayAlert("Save Failed", result.ErrorMessage, "OK");                    
                    return;
                }

                //await Shell.Current.DisplayAlert("Success", isNew ? "Customer added" : "Customer updated", "OK");
                _actionMessageService.Set(isNew ? "Customer added." : "Customer updated.");
                await Shell.Current.GoToAsync("..");    
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool Validate()
        {
            string username = Username.Trim();
            string contact = Contact.Trim();
            string email = Email.Trim();

            UsernameError = string.Empty;
            AgeError = string.Empty;
            ContactError = string.Empty;
            EmailError = string.Empty;

            if (username.Length == 0)
                UsernameError = "Username is required";
            else if (username.Length < 3 || username.Length > 50)
                UsernameError = "Username must be 3 to 50 characters";
            else if (!UsernamePattern.IsMatch(username))
                UsernameError = "Username can only include letters, numbers and underscores";

            if (!int.TryParse(AgeText.Trim(), out int age))
                AgeError = "Age is required and must be a number";
            else if (age < 1 || age > 120)
                AgeError = "Age must be in between 1 and 120";

            if (contact.Length == 0)
                ContactError = "Contact is required";
            else if (!ContactPattern.IsMatch(contact))
                ContactError = "Contact can only include 8 to 11 numbers and optional leading +";

            if (email.Length == 0)
                EmailError = "Email is required";
            else if (email.Length > 50)
                EmailError = "Email cannot exceed 50 characters.";
            else if (!EmailPattern.IsMatch(email))
                EmailError = "Please follow email format (eg. example@gmail.com / example@gmail.com.my)";

            return !HasUsernameError && !HasAgeError && !HasContactError && !HasEmailError;
        }
    }
}
