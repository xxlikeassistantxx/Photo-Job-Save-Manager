using System.Collections.ObjectModel;
using System.Windows.Input;
using Photo_Job_Save_Manager.Models;
using Photo_Job_Save_Manager.Services;
using Microsoft.Maui.Storage;
using System.Text.Json;
using CommunityToolkit.Mvvm.Messaging;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class CloudStorageViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        public ObservableCollection<Job> CloudJobs { get; } = new();
        public ObservableCollection<JobType> JobTypes { get; }
        
        public ICommand RefreshCommand { get; }
        public ICommand DownloadJobCommand { get; }
        public ICommand DeleteFromCloudCommand { get; }
        public ICommand UploadJobCommand { get; }

        private bool _isRefreshing;
        public bool IsRefreshing 
        { 
            get => _isRefreshing; 
            set => SetProperty(ref _isRefreshing, value); 
        }

        private string _statusMessage = "Ready";
        public string StatusMessage 
        { 
            get => _statusMessage; 
            set => SetProperty(ref _statusMessage, value); 
        }

        public CloudStorageViewModel(IAuthService authService, JobTypesViewModel jobTypesViewModel)
        {
            _authService = authService;
            JobTypes = jobTypesViewModel.JobTypes;
            Title = "Cloud Storage";
            
            RefreshCommand = new Command(async () => await RefreshCloudJobsAsync());
            DownloadJobCommand = new Command<Job>(async (job) => await DownloadJobAsync(job));
            DeleteFromCloudCommand = new Command<Job>(async (job) => await DeleteFromCloudAsync(job));
            UploadJobCommand = new Command<Job>(async (job) => await UploadJobAsync(job));
            
            // Load initial data
            _ = Task.Run(async () => await RefreshCloudJobsAsync());
        }

        private async Task RefreshCloudJobsAsync()
        {
            try
            {
                IsRefreshing = true;
                StatusMessage = "Connecting to cloud storage...";
                
                var currentUser = await _authService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    StatusMessage = "Please log in to access cloud storage";
                    return;
                }

                StatusMessage = "Loading jobs from cloud...";
                
                // Test Firebase connectivity first
                var connectivityResult = await TestFirebaseConnectivity();
                if (!connectivityResult.isConnected)
                {
                    StatusMessage = $"Cloud connection failed: {connectivityResult.error}";
                    return;
                }

                StatusMessage = "Fetching your cloud jobs...";
                
                // Here we would integrate with Firebase to fetch jobs
                // For now, we'll simulate cloud jobs or load from a cloud cache
                await LoadCloudJobsFromCache();
                
                StatusMessage = $"Loaded {CloudJobs.Count} jobs from cloud storage";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CloudStorageViewModel: Loaded {CloudJobs.Count} cloud jobs");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading cloud jobs: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CloudStorageViewModel: Error refreshing: {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task<(bool isConnected, string error)> TestFirebaseConnectivity()
        {
            try
            {
                // Simulate connectivity test - in real implementation this would ping Firebase
                await Task.Delay(1000);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task LoadCloudJobsFromCache()
        {
            try
            {
                // Load cloud jobs from local cache (jobs that have been uploaded to cloud)
                var json = Preferences.Get("cloud_jobs_cache", null);
                if (!string.IsNullOrEmpty(json))
                {
                    var jobs = JsonSerializer.Deserialize<List<Job>>(json);
                    if (jobs != null)
                    {
                        CloudJobs.Clear();
                        foreach (var job in jobs)
                        {
                            CloudJobs.Add(job);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CloudStorageViewModel: Error loading cache: {ex.Message}");
            }
        }

        private void SaveCloudJobsCache()
        {
            try
            {
                var json = JsonSerializer.Serialize(CloudJobs);
                Preferences.Set("cloud_jobs_cache", json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CloudStorageViewModel: Error saving cache: {ex.Message}");
            }
        }

        private async Task DownloadJobAsync(Job job)
        {
            try
            {
                StatusMessage = $"Downloading job '{job.JobName}'...";
                
                // Simulate download process
                await Task.Delay(1500);
                
                // Add to local saved jobs
                WeakReferenceMessenger.Default.Send(new JobSavedMessage(job));
                
                StatusMessage = $"Job '{job.JobName}' downloaded successfully";
                
                await Application.Current.MainPage.DisplayAlert(
                    "Download Complete",
                    $"Job '{job.JobName}' has been downloaded to your device.",
                    "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Download failed: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "Download Failed",
                    $"Could not download job '{job.JobName}': {ex.Message}",
                    "OK");
            }
        }

        private async Task DeleteFromCloudAsync(Job job)
        {
            try
            {
                var confirm = await Application.Current.MainPage.DisplayAlert(
                    "Delete from Cloud",
                    $"Are you sure you want to delete '{job.JobName}' from cloud storage? This cannot be undone.",
                    "Delete",
                    "Cancel");

                if (!confirm) return;

                StatusMessage = $"Deleting job '{job.JobName}' from cloud...";
                
                // Simulate cloud deletion
                await Task.Delay(1000);
                
                CloudJobs.Remove(job);
                SaveCloudJobsCache();
                
                StatusMessage = $"Job '{job.JobName}' deleted from cloud";
                
                await Application.Current.MainPage.DisplayAlert(
                    "Deleted",
                    $"Job '{job.JobName}' has been deleted from cloud storage.",
                    "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Delete failed: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "Delete Failed",
                    $"Could not delete job '{job.JobName}': {ex.Message}",
                    "OK");
            }
        }

        private async Task UploadJobAsync(Job job)
        {
            try
            {
                StatusMessage = $"Uploading job '{job.JobName}' to cloud...";
                
                // Simulate upload process
                await Task.Delay(2000);
                
                // Add to cloud jobs if not already there
                if (!CloudJobs.Any(cj => cj.Id == job.Id))
                {
                    CloudJobs.Add(job);
                    SaveCloudJobsCache();
                }
                
                StatusMessage = $"Job '{job.JobName}' uploaded successfully";
                
                await Application.Current.MainPage.DisplayAlert(
                    "Upload Complete",
                    $"Job '{job.JobName}' has been uploaded to cloud storage.",
                    "OK");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Upload failed: {ex.Message}";
                await Application.Current.MainPage.DisplayAlert(
                    "Upload Failed",
                    $"Could not upload job '{job.JobName}': {ex.Message}",
                    "OK");
            }
        }

        public async Task UploadJobFromSavedJobs(Job job)
        {
            await UploadJobAsync(job);
        }
    }
} 