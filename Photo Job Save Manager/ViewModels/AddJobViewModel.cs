using System.Collections.ObjectModel;
using System.Windows.Input;
using Photo_Job_Save_Manager.Models;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Media;
using System.Diagnostics;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class AddJobViewModel : BaseViewModel
    {
        public string JobTypeName { get; set; } = string.Empty;
        public ObservableCollection<FieldEntryViewModel> FieldEntries { get; } = new();
        public ICommand SaveCommand { get; }
        public string Status { get; set; } = string.Empty;

        private JobTypesViewModel? _jobTypesViewModel;

        public AddJobViewModel()
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobViewModel: Constructor called");
            SaveCommand = new Command(async () => await SaveAsync());
            // Get the JobTypesViewModel once during construction
            _jobTypesViewModel = App.Current.Services.GetService<JobTypesViewModel>();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: JobTypesViewModel obtained: {_jobTypesViewModel != null}");
            if (_jobTypesViewModel != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: JobTypes count in constructor: {_jobTypesViewModel.JobTypes.Count}");
            }
        }

        public void LoadJobType(string jobTypeName)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: LoadJobType called with: {jobTypeName}");
            
            // Ensure we have the JobTypesViewModel
            if (_jobTypesViewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobViewModel: JobTypesViewModel was null, getting new instance");
                _jobTypesViewModel = App.Current.Services.GetService<JobTypesViewModel>();
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: JobTypesViewModel was null, got new instance: {_jobTypesViewModel != null}");
            }
            
            if (_jobTypesViewModel == null)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] AddJobViewModel: Failed to get JobTypesViewModel from service container");
                return;
            }
            
            var jobTypes = _jobTypesViewModel.JobTypes.ToList();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: Available job types: {string.Join(", ", jobTypes.Select(jt => jt.Name))}");
            
            FieldEntries.Clear();
            var jobType = jobTypes.FirstOrDefault(j => 
                j.Name.Trim().Equals(jobTypeName?.Trim(), StringComparison.OrdinalIgnoreCase));
            
            System.Diagnostics.Debug.WriteLine(jobType == null ? "[DEBUG] AddJobViewModel: JobType not found!" : $"[DEBUG] AddJobViewModel: JobType found: {jobType.Name}");
            
            if (jobType != null)
            {
                JobTypeName = jobType.Name;
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: Setting JobTypeName to: {JobTypeName}");
                foreach (var field in jobType.Fields)
                {
                    Debug.WriteLine($"[DEBUG] AddJobViewModel: Adding field: {field.Name}, Type: {field.FieldType}");
                    FieldEntries.Add(new FieldEntryViewModel(field));
                }
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: Total field entries added: {FieldEntries.Count}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AddJobViewModel: JobType '{jobTypeName}' not found in available types: {string.Join(", ", jobTypes.Select(jt => $"'{jt.Name}'"))}");
            }
        }

        private async Task SaveAsync()
        {
            // Build the Job object
            var job = new Photo_Job_Save_Manager.Models.Job
            {
                Id = Guid.NewGuid().ToString(),
                JobTypeId = JobTypeName,
                UserId = "", // TODO: set to current user when auth is integrated
                CreatedAt = DateTime.UtcNow,
                Data = FieldEntries.ToDictionary(f => f.Field.Name, f =>
                {
                    if (f.Field.FieldType == JobFieldType.Photo)
                        return (object?)f.PhotoFilePaths.ToList();
                    else if (f.Field.FieldType == JobFieldType.Dropdown)
                        return (object?)f.SelectedValue ?? "";
                    else
                        return (object?)f.Value ?? "";
                })
            };
            // Send the job to listeners (e.g., SavedJobsViewModel)
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(new JobSavedMessage(job));
            // Show confirmation
            var summary = string.Join("\n", FieldEntries.Select(f =>
            {
                if (f.Field.FieldType == JobFieldType.Photo)
                    return $"{f.Field.Name}: {(f.PhotoFilePaths.Count > 0 ? string.Join(", ", f.PhotoFilePaths.Select(System.IO.Path.GetFileName)) : "<none>")}";
                else if (f.Field.FieldType == JobFieldType.Dropdown)
                    return $"{f.Field.Name}: {f.SelectedValue ?? "<none>"}";
                else
                    return $"{f.Field.Name}: {f.Value ?? "<none>"}";
            }));
            await Application.Current.MainPage.DisplayAlert("Job Saved", summary, "OK");
            await Shell.Current.GoToAsync("..", true);
        }
    }

    public class FieldEntryViewModel : BaseViewModel
    {
        public JobTypeField Field { get; }
        public string? Value { get; set; }
        public string? SelectedValue { get; set; }
        public ObservableCollection<string> PhotoFilePaths { get; } = new();
        public ICommand PickPhotoCommand { get; }
        public ICommand TakePhotoCommand { get; }
        public ICommand RemovePhotoCommand { get; }

        public FieldEntryViewModel(JobTypeField field)
        {
            Field = field;
            PickPhotoCommand = new Command(async () => await PickPhotoAsync());
            TakePhotoCommand = new Command(async () => await TakePhotoAsync());
            RemovePhotoCommand = new Command<string>(RemovePhoto);
        }

        private async Task PickPhotoAsync()
        {
            try
            {
                var result = await Microsoft.Maui.Media.MediaPicker.Default.PickPhotoAsync();
                if (result != null)
                {
                    PhotoFilePaths.Add(result.FullPath);
                    OnPropertyChanged(nameof(PhotoFilePaths));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] PickPhotoAsync exception: {ex.Message}");
            }
        }

        private async Task TakePhotoAsync()
        {
            try
            {
                var result = await Microsoft.Maui.Media.MediaPicker.Default.CapturePhotoAsync();
                if (result != null)
                {
                    PhotoFilePaths.Add(result.FullPath);
                    OnPropertyChanged(nameof(PhotoFilePaths));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] TakePhotoAsync exception: {ex.Message}");
            }
        }

        private void RemovePhoto(string filePath)
        {
            if (PhotoFilePaths.Contains(filePath))
                PhotoFilePaths.Remove(filePath);
            OnPropertyChanged(nameof(PhotoFilePaths));
        }
    }

    public class JobSavedMessage
    {
        public JobSavedMessage(Photo_Job_Save_Manager.Models.Job job) => Job = job;
        public Photo_Job_Save_Manager.Models.Job Job { get; }
    }
} 