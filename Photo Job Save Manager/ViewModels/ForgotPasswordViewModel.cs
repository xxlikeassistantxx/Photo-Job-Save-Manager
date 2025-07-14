using Photo_Job_Save_Manager.Services;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class ForgotPasswordViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private string _email = string.Empty;
        private string _message = string.Empty;
        private bool _isMessageVisible = false;
        private bool _isSuccessMessage = false;

        public ForgotPasswordViewModel(IAuthService authService)
        {
            _authService = authService;
            Title = "Forgot Password";
            
            SendResetCommand = new Command(async () => await SendResetAsync());
            BackToLoginCommand = new Command(async () => await BackToLoginAsync());
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
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

        public ICommand SendResetCommand { get; }
        public ICommand BackToLoginCommand { get; }

        private async Task SendResetAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                ClearMessage();

                // Validate email
                if (string.IsNullOrWhiteSpace(Email))
                {
                    ShowMessage("Please enter your email address.", false);
                    return;
                }

                if (!IsValidEmail(Email))
                {
                    ShowMessage("Please enter a valid email address.", false);
                    return;
                }

                // Send password reset email
                var success = await _authService.SendPasswordResetEmailAsync(Email);
                
                if (success)
                {
                    ShowMessage("Password reset link sent! Please check your email.", true);
                    
                    // Navigate back to login after a delay
                    await Task.Delay(2000);
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    ShowMessage("Failed to send reset link. Please try again.", false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Password reset failed: {ex.Message}", false);
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