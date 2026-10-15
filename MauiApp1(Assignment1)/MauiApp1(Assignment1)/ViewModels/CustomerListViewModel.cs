using MauiApp1_Assignment1_.Models;
using MauiApp1_Assignment1_.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiApp1_Assignment1_.ViewModels
{
    public class CustomerListViewModel : BaseViewModel
    {
        private const int SearchDelayMilliseconds = 300;

        private readonly CustomerService _customerService;
        private readonly AuthService _authService;

        private readonly ActionMessageService _actionMessageService;
        private string _actionMessage = string.Empty;
        private int _messageId;

        private string _searchText = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isRefreshing;
        private int _requestCounter;
        private CancellationTokenSource? _searchCts;

        public CustomerListViewModel(CustomerService customerService, AuthService authService, ActionMessageService actionMessageService)
        {
            _customerService = customerService;
            _authService = authService;
            _actionMessageService = actionMessageService;

            RefreshCommand = new Command(async () => await LoadAsync());
            AddCommand = new Command(async () => await Shell.Current.GoToAsync(Routes.CustomerEdit));
            EditCommand = new Command<Customer>(async customer => await EditAsync(customer));
            DeleteCommand = new Command<Customer>(async customer => await DeleteSync(customer));
            LogoutCommand = new Command(async () => await LogoutAsync());
        }

        public ObservableCollection<Customer> Customers { get; } = new();

        //Typing here starts a search after a short pause
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    ScheduleSearch();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            private set
            {
                if (SetProperty(ref _statusMessage, value))
                    OnPropertyChanged(nameof(HasStatusMessage));
            }
        }

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        public string ActionMessage
        {
            get => _actionMessage;
            private set
            {
                if (SetProperty(ref _actionMessage, value))
                    OnPropertyChanged(nameof(HasActionMessage));
            }
        }

        public bool HasActionMessage => !string.IsNullOrEmpty(ActionMessage);

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }

        // Called when the page appears: shows the note left by the edit page, if any
        public void ShowPendingMessage()
        {
            string? message = _actionMessageService.Take();
            if (!string.IsNullOrEmpty(message))
                ShowMessage(message);
        }

        public async void ShowMessage(string message)
        {
            int id = ++_messageId;
            ActionMessage = message;

            await Task.Delay(3000);

            //Only hide it if no newer message has replaced it
            if (id == _messageId)
                ActionMessage = string.Empty;
        }

        public ICommand RefreshCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand LogoutCommand { get; }

        // Load the list(all customers, or the ones matching SearchText)
        public async Task LoadAsync()
        {
            int requestId = ++_requestCounter;
            IsBusy = true;

            try
            {
                ApiResult<List<Customer>> result = await _customerService.GetAllAsync(SearchText);

                //A newer search has started since this one, so ignore this old answer
                if (requestId != _requestCounter)
                    return;

                if (result.IsUnauthorized)
                {
                    await GoToLoginAsync();
                    return;
                }

                Customers.Clear();

                if (!result.Success)
                {
                    StatusMessage = result.ErrorMessage;
                    return;
                }

                foreach (Customer customer in result.Data ?? new List<Customer>())
                    Customers.Add(customer);

                bool isSearching = !string.IsNullOrWhiteSpace(SearchText);

                if (Customers.Count == 0)
                    StatusMessage = isSearching ? $"No customers found for \"{SearchText.Trim()}\"." : "No customers yet.";
                else
                    StatusMessage = isSearching ? $"{Customers.Count} customer(s) found." : string.Empty;
            }
            finally
            {
                if (requestId == _requestCounter)
                    IsBusy = false;

                IsRefreshing = false;
            }
        }

        //Wait 300ms after the last key press, so typing "john" sends one search, not four
        private async void ScheduleSearch()
        {
            _searchCts?.Cancel();
            _searchCts  = new CancellationTokenSource();
            CancellationToken token = _searchCts.Token;

            try
            {
                await Task.Delay(SearchDelayMilliseconds, token);
                await LoadAsync();
            }
            catch (TaskCanceledException)
            {
                //The user typed again, so this
                //is replaced by a newer one
            }
            catch(Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private async Task EditAsync(Customer? customer)
        {
            if (customer is null)
                return;

            await Shell.Current.GoToAsync(Routes.CustomerEdit, new ShellNavigationQueryParameters
            {
                { "Customer", customer }
            });
        }

        private async Task DeleteSync(Customer? customer)
        {
            if (customer is null)
                return;
            bool comfirmed = await Shell.Current.DisplayAlert("Delete customer", $"Delete {customer.Username} ?", "Yes", "No");

            if (!comfirmed)
                return;

            ApiResult result = await _customerService.DeleteAsync(customer.CustomerId);

            if (result.IsUnauthorized)
            {
                await GoToLoginAsync();
                return;
            }

            if (!result.Success)
            {
                await Shell.Current.DisplayAlert("Delete failed", result.ErrorMessage, "OK");
                return;
            }
            ShowMessage($"{customer.Username} was deleted.");
            await LoadAsync();
        }

        private async Task LogoutAsync()
        {
            await GoToLoginAsync();
        }

        private async Task GoToLoginAsync()
        {
            _authService.Logout();
            Customers.Clear();
            SearchText = string.Empty;
            StatusMessage = string.Empty;
            await Shell.Current.GoToAsync(Routes.LoginAbsolute);

        }
    }
}
