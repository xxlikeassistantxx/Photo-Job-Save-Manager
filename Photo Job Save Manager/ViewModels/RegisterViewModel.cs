using Photo_Job_Save_Manager.Services;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _message = string.Empty;
        private bool _isMessageVisible = false;
        private bool _isSuccessMessage = false;

        public RegisterViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Register";
            
            RegisterCommand = new Command(async () => await RegisterAsync());
            BackToLoginCommand = new Command(async () => await BackToLoginAsync());
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool IsMessageVisible
        {
            get => _isMessageVisible;
            set => SetProperty(ref _isMessageVisible, value);
        }

        public bool IsSuccessMessage
        {
            get => _isSuccessMessage;
            set => SetProperty(ref _isSuccessMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackToLoginCommand { get; }

        private async Task RegisterAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearMessage();

                // Validate inputs
                if (string.IsNullOrWhiteSpace(Username))
                {
                    ShowMessage("Please enter a username.", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Email))
                {
                    ShowMessage("Please enter your email address.", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowMessage("Please enter a password.", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    ShowMessage("Please confirm your password.", false);
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    ShowMessage("Please enter a valid email address.", false);
                    return;
                }

                if (!IsValidPassword(Password))
                {
                    ShowMessage("Password must be 8-17 characters long and contain at least one uppercase letter, one number, and one special character.", false);
                    return;
                }

                if (Password != ConfirmPassword)
                {
                    ShowMessage("Passwords do not match.", false);
                    return;
                }

                // Attempt registration
                var success = await _authService.RegisterAsync(Username, Email, Password);
                
                if (success)
                {
                    ShowMessage("Registration successful! Please check your email for a verification link before signing in.", true);
                    // Do not navigate away immediately; let the user see the message and check their email
                }
                else
                {
                    ShowMessage("Registration failed. Please try again.", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task BackToLoginAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPassword(string password)
        {
            // Password requirements: 8-17 characters, 1 uppercase, 1 number, 1 special character
            if (password.Length < 8 || password.Length > 17)
                return false;

            var hasUpperCase = password.Any(char.IsUpper);
            var hasNumber = password.Any(char.IsDigit);
            var hasSpecialChar = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpperCase && hasNumber && hasSpecialChar;
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            Message = message;
            IsSuccessMessage = isSuccess;
            IsMessageVisible = true;
        }

        private void ClearMessage()
        {
            Message = string.Empty;
            IsMessageVisible = false;
        }
    }
} 