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

        private List<JobType> _jobTypes => App.Current.Services.GetService<JobTypesViewModel>()?.JobTypes.ToList() ?? new();

        public AddJobViewModel()
        {
            SaveCommand = new Command(async () => await SaveAsync());
        }

        public void LoadJobType(string jobTypeName)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadJobType called with: {jobTypeName}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Available job types: {string.Join(", ", _jobTypes.Select(jt => jt.Name))}");
            FieldEntries.Clear();
            var jobType = _jobTypes.FirstOrDefault(j => 
                j.Name.Trim().Equals(jobTypeName?.Trim(), StringComparison.OrdinalIgnoreCase));
            System.Diagnostics.Debug.WriteLine(jobType == null ? "[DEBUG] JobType not found!" : $"[DEBUG] JobType found: {jobType.Name}");
            if (jobType != null)
            {
                JobTypeName = jobType.Name;
                foreach (var field in jobType.Fields)
                {
                    Debug.WriteLine($"[DEBUG] Adding field: {field.Name}, Type: {field.FieldType}");
                    FieldEntries.Add(new FieldEntryViewModel(field));
                }
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