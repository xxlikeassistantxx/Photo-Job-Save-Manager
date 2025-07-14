using Photo_Job_Save_Manager.Models;
using Photo_Job_Save_Manager.ViewModels;
using System.Windows.Input;

namespace Photo_Job_Save_Manager.Views
{
    [QueryProperty(nameof(JobTypeName), "JobTypeName")]
    public partial class AddJobPage : ContentPage
    {
        public string JobTypeName
        {
            get => _jobTypeName;
            set
            {
                _jobTypeName = value;
                System.Diagnostics.Debug.WriteLine($"JobTypeName property set: {_jobTypeName}");
                if (BindingContext is AddJobViewModel vm)
                    vm.LoadJobType(_jobTypeName);
            }
        }
        private string _jobTypeName;

        public AddJobPage(AddJobViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            ShowPhotoCommand = new Command<string>(async (filePath) => await ShowPhotoAsync(filePath));
        }

        public ICommand ShowPhotoCommand { get; }

        private async Task ShowPhotoAsync(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                await Shell.Current.GoToAsync($"PhotoViewerPage?filePath={Uri.EscapeDataString(filePath)}");
            }
        }

        public AddJobPage() : this(App.Current.Services.GetService<AddJobViewModel>()) { }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobPage: Back button clicked");
            try {
                System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobPage: Trying Shell.Current.GoToAsync('..')");
                await Shell.Current.GoToAsync("..");
                System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobPage: Shell.Current.GoToAsync('..') succeeded");
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobPage: GoToAsync failed: {ex.Message}");
                if (Shell.Current.Navigation.NavigationStack.Count > 1) {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobPage: Trying Shell.Current.Navigation.PopAsync()");
                    await Shell.Current.Navigation.PopAsync();
                    System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobPage: PopAsync succeeded");
                } else {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobPage: PopAsync not possible, NavigationStack.Count <= 1");
                }
            }
        }
    }
} 