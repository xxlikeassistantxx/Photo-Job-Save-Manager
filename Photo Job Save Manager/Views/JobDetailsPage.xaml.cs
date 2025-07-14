using Microsoft.Maui.Controls;
using Photo_Job_Save_Manager.Models;
using Photo_Job_Save_Manager.ViewModels;
using System.Linq;
using System.Windows.Input;
using Photo_Job_Save_Manager.Services;

namespace Photo_Job_Save_Manager.Views
{
    [QueryProperty(nameof(JobId), "jobId")]
    public partial class JobDetailsPage : ContentPage
    {
        private string _jobId;
        public string JobId
        {
            get => _jobId;
            set
            {
                _jobId = value;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobDetailsPage navigated to with jobId: {_jobId}");
                LoadJob(_jobId);
            }
        }

        public ICommand ShowPhotoCommand { get; }

        public JobDetailsPage()
        {
            InitializeComponent();
            ShowPhotoCommand = new Command<string>(async (filePath) => await ShowPhotoAsync(filePath));
        }

        private void LoadJob(string jobId)
        {
            var savedJobsVm = App.Current.Services.GetService<SavedJobsViewModel>();
            var job = savedJobsVm?.AllJobs.FirstOrDefault(j => j.Id == jobId);
            if (job != null)
            {
                BindingContext = new JobDetailsViewModel(job);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Job not found for id: {jobId}");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] JobDetailsPage: Back button clicked");
            try {
                System.Diagnostics.Debug.WriteLine("[DEBUG] JobDetailsPage: Trying Shell.Current.GoToAsync('..')");
                await Shell.Current.GoToAsync("..");
                System.Diagnostics.Debug.WriteLine("[DEBUG] JobDetailsPage: Shell.Current.GoToAsync('..') succeeded");
            } catch (Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] JobDetailsPage: GoToAsync failed: {ex.Message}");
                if (Shell.Current.Navigation.NavigationStack.Count > 1) {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] JobDetailsPage: Trying Shell.Current.Navigation.PopAsync()");
                    await Shell.Current.Navigation.PopAsync();
                    System.Diagnostics.Debug.WriteLine("[DEBUG] JobDetailsPage: PopAsync succeeded");
                } else {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] JobDetailsPage: PopAsync not possible, NavigationStack.Count <= 1");
                }
            }
        }

        private async void OnCloudSyncClicked(object sender, EventArgs e)
        {
            if (BindingContext is JobDetailsViewModel vm)
            {
                try
                {
                    // TODO: Replace with actual retrieval of authToken, userId, firebaseUrl, firebaseStorage, apiKey
                    string email = "your@email.com";
                    string password = "yourpassword";
                    string apiKey = "your-firebase-api-key";
                    string firebaseUrl = "https://<your-project-id>.firebaseio.com/";
                    string firebaseStorage = "<your-project-id>.appspot.com";

                    var (authToken, userId) = await FirebaseService.AuthenticateAsync(email, password, apiKey);
                    var firebaseService = new FirebaseService(authToken, userId, firebaseUrl);
                    await firebaseService.UploadJobAsync(vm.Job);
                    await DisplayAlert("Cloud Sync", "Job uploaded to cloud!", "OK");
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Cloud Sync Failed", ex.Message, "OK");
                }
            }
        }

        private async Task ShowPhotoAsync(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                await Shell.Current.GoToAsync($"PhotoViewerPage?filePath={Uri.EscapeDataString(filePath)}");
            }
        }
    }

    public class JobDetailsViewModel
    {
        public Job Job { get; }
        public List<string> PhotoFilePaths { get; }
        public List<FieldDisplayViewModel> Fields { get; }
        public ICommand ShowPhotoCommand { get; }
        public JobDetailsViewModel(Job job)
        {
            Job = job;
            var jobTypesVm = App.Current.Services.GetService<Photo_Job_Save_Manager.ViewModels.JobTypesViewModel>();
            var jobType = jobTypesVm?.JobTypes.FirstOrDefault(jt => jt.Name == job.JobTypeId);
            PhotoFilePaths = new List<string>();
            Fields = new List<FieldDisplayViewModel>();
            if (jobType != null)
            {
                foreach (var field in jobType.Fields)
                {
                    if (field.FieldType == Photo_Job_Save_Manager.Models.JobFieldType.Photo &&
                        job.Data.TryGetValue(field.Name, out var value) &&
                        value is List<string> paths && paths.Count > 0)
                    {
                        PhotoFilePaths.AddRange(paths);
                        Fields.Add(new FieldDisplayViewModel {
                            Name = field.Name,
                            FieldType = field.FieldType,
                            PhotoPaths = paths
                        });
                    }
                    else if (field.FieldType == Photo_Job_Save_Manager.Models.JobFieldType.Text &&
                        job.Data.TryGetValue(field.Name, out var textValue))
                    {
                        Fields.Add(new FieldDisplayViewModel {
                            Name = field.Name,
                            FieldType = field.FieldType,
                            TextValue = textValue?.ToString() ?? string.Empty
                        });
                    }
                }
            }
            ShowPhotoCommand = new Command<string>(async (filePath) => await ShowPhotoAsync(filePath));
        }
        private async Task ShowPhotoAsync(string filePath)
        {
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                await Shell.Current.GoToAsync($"PhotoViewerPage?filePath={Uri.EscapeDataString(filePath)}");
            }
        }
    }

    public class FieldDisplayViewModel
    {
        public string Name { get; set; } = string.Empty;
        public Photo_Job_Save_Manager.Models.JobFieldType FieldType { get; set; }
        public string TextValue { get; set; } = string.Empty;
        public List<string> PhotoPaths { get; set; } = new();
    }
} 