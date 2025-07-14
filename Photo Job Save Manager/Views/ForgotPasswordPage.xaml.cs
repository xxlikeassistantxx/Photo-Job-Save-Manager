using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class ForgotPasswordPage : ContentPage
    {
        public ForgotPasswordPage(ForgotPasswordViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private void OnEmailCompleted(object sender, EventArgs e)
        {
            if (BindingContext is ForgotPasswordViewModel viewModel)
            {
                viewModel.SendResetCommand.Execute(null);
            }
        }
    }
} 