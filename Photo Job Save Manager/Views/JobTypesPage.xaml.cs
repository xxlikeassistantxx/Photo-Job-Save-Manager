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
            System.Diagnostics.Debug.WriteLine("[DEBUG] JobTypesPage: OnJobTypeSelected called");
            if (e.CurrentSelection.FirstOrDefault() is Photo_Job_Save_Manager.Models.JobType jobType)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesPage: Selected job type: {jobType.Name}");
                var route = $"{nameof(Views.AddJobPage)}?JobTypeName={Uri.EscapeDataString(jobType.Name)}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesPage: Navigating to: {route}");
                await Shell.Current.GoToAsync(route);
                System.Diagnostics.Debug.WriteLine("[DEBUG] JobTypesPage: Navigation completed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] JobTypesPage: No job type selected");
            }
            ((CollectionView)sender).SelectedItem = null;
        }

        private async void OnJobTypeTapped(object sender, TappedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] JobTypesPage: OnJobTypeTapped called");
            // Handle tap gesture on job type items
            if (sender is CollectionView collectionView && e.Parameter is Photo_Job_Save_Manager.Models.JobType jobType)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesPage: Tapped job type: {jobType.Name}");
                var route = $"{nameof(Views.AddJobPage)}?JobTypeName={Uri.EscapeDataString(jobType.Name)}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobTypesPage: Navigating to: {route}");
                await Shell.Current.GoToAsync(route);
                System.Diagnostics.Debug.WriteLine("[DEBUG] JobTypesPage: Navigation completed");
            }
        }
    }
} 