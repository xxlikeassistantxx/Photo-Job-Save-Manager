using Photo_Job_Save_Manager.ViewModels;

namespace Photo_Job_Save_Manager.Views
{
    public partial class JobTypesPage : ContentPage
    {
        public JobTypesPage(JobTypesViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        public JobTypesPage() : this(((App)Application.Current).Services.GetService<JobTypesViewModel>()) { }

        private async void OnJobTypeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is Photo_Job_Save_Manager.Models.JobType jobType)
            {
                await Shell.Current.GoToAsync($"{nameof(Views.AddJobPage)}?JobTypeName={Uri.EscapeDataString(jobType.Name)}");
            }
            ((CollectionView)sender).SelectedItem = null;
        }
    }
} 