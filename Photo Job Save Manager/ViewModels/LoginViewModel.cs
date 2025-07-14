using Photo_Job_Save_Manager.Services;
using Photo_Job_Save_Manager.Models;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;
        private bool _isMessageVisible = false;
        private bool _isSuccessMessage = false;
        private bool _showResendVerification = false;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Login";
            LoadLastLoginAsync();
            LoginCommand = new Command(async () => await LoginAsync());
            RegisterCommand = new Command(async () => await RegisterAsync());
            ForgotPasswordCommand = new Command(async () => await ForgotPasswordAsync());
            ResendVerificationCommand = new Command(async () => await ResendVerificationAsync());
        }

        private async void LoadLastLoginAsync()
        {
            try
            {
                Email = await SecureStorage.GetAsync("last_email") ?? string.Empty;
                Password = await SecureStorage.GetAsync("last_password") ?? string.Empty;
            }
            catch { /* ignore */ }
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

        public bool ShowResendVerification
        {
            get => _showResendVerification;
            set => SetProperty(ref _showResendVerification, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand ForgotPasswordCommand { get; }
        public ICommand ResendVerificationCommand { get; }

        private async Task LoginAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearMessage();
                ShowResendVerification = false;

                // Validate inputs
                if (string.IsNullOrWhiteSpace(Email))
                {
                    ShowMessage("Please enter your email address.", false);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Password))
                {
                    ShowMessage("Please enter your password.", false);
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    ShowMessage("Please enter a valid email address.", false);
                    return;
                }

                // Attempt login
                var success = await _authService.LoginAsync(Email, Password);
                
                if (success)
                {
                    // Save last login info
                    await SecureStorage.SetAsync("last_email", Email);
                    await SecureStorage.SetAsync("last_password", Password);
                    ShowMessage("Login successful! Redirecting...", true);
                    
                    // Check if email is verified
                    var isEmailVerified = await _authService.IsEmailVerifiedAsync();
                    if (!isEmailVerified)
                    {
                        ShowMessage("Please verify your email address before logging in.", false);
                        ShowResendVerification = true;
                        return;
                    }

                    // Navigate to main page after a short delay
                    await Task.Delay(1000);
                    Application.Current.MainPage = new AppShell();
                    await Shell.Current.GoToAsync("//MainDashboardPage");
                }
                else
                {
                    ShowMessage("Invalid email or password. Please try again.", false);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("verify your email address"))
                {
                    ShowResendVerification = true;
                }
                ShowMessage($"Login failed: {ex.Message}", false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ResendVerificationAsync()
        {
            if (IsBusy) return;
            try
            {
                IsBusy = true;
                ClearMessage();
                var sent = await _authService.VerifyEmailAsync();
                if (sent)
                {
                    ShowMessage("Verification email sent! Please check your inbox.", true);
                }
                else
                {
                    ShowMessage("Failed to send verification email. Please try again later.", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Failed to send verification email: {ex.Message}", false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RegisterAsync()
        {
            await Shell.Current.GoToAsync("RegisterPage");
        }

        private async Task ForgotPasswordAsync()
        {
            await Shell.Current.GoToAsync("ForgotPasswordPage");
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