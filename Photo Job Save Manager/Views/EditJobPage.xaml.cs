using System.Windows.Input;
using Photo_Job_Save_Manager.ViewModels;
using Photo_Job_Save_Manager.Models;
using Microsoft.Maui.Controls;

namespace Photo_Job_Save_Manager.Views
{
    [QueryProperty(nameof(JobId), "jobId")]
    public partial class EditJobPage : ContentPage
    {
        private string? _jobId;
        public string? JobId
        {
            get => _jobId;
            set
            {
                _jobId = value;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobPage - JobId property set: {_jobId}");
                if (!string.IsNullOrEmpty(_jobId))
                {
                    // Fire and forget, since this is a property setter
                    _ = LoadJobById(_jobId);
                }
            }
        }

        public EditJobPage()
        {
            InitializeComponent();
            BindingContext = new EditJobViewModel();
        }

        public EditJobPage(string jobId) : this()
        {
            _jobId = jobId;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobPage constructor - jobId: {jobId}");
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            // No need to extract jobId here; handled by property
        }

        private async Task LoadJobById(string jobId)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobPage - Loading job with ID: {jobId}");
            
            // Get the SavedJobsViewModel to find the job
            var savedJobsViewModel = App.Current.Services.GetService<SavedJobsViewModel>();
            if (savedJobsViewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobPage - Found SavedJobsViewModel, AllJobs count: {savedJobsViewModel.AllJobs.Count}");
                
                var job = savedJobsViewModel.AllJobs.FirstOrDefault(j => j.Id == jobId);
                if (job != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobPage - Found job: {job.JobTypeId}");
                    if (BindingContext is EditJobViewModel editViewModel)
                    {
                        editViewModel.LoadJob(job);
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobPage - Job loaded into EditJobViewModel");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[DEBUG] EditJobPage - BindingContext is not EditJobViewModel");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] EditJobPage - Job with ID {jobId} not found");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] EditJobPage - SavedJobsViewModel not found");
            }
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("..");
        }

        public ICommand ShowPhotoCommand => new Command<string>(async (photoPath) =>
        {
            if (!string.IsNullOrEmpty(photoPath))
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Photo", $"Photo path: {photoPath}", "OK");
            }
        });
    }
} 