using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using Photo_Job_Save_Manager.Models;
using System.Linq;

namespace Photo_Job_Save_Manager.ViewModels
{
    public class JobTypeParameterViewModel : BaseViewModel
    {
        public string JobTypeName { get => _jobTypeName; set => SetProperty(ref _jobTypeName, value); }
        private string _jobTypeName = string.Empty;
        public ObservableCollection<JobTypeField> Fields { get; } = new();
        public ICommand AddFieldCommand { get; }
        public ICommand RemoveFieldCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddAllowedValueCommand { get; }
        public ICommand RemoveAllowedValueCommand { get; }
        public List<JobFieldType> AvailableFieldTypes { get; } = new() { JobFieldType.Text, JobFieldType.Photo, JobFieldType.Dropdown };

        public JobTypeParameterViewModel()
        {
            AddFieldCommand = new Command(AddField);
            RemoveFieldCommand = new Command<JobTypeField>(RemoveField);
            SaveCommand = new Command(async () => await SaveAsync());
            AddAllowedValueCommand = new Command<JobTypeField>(AddAllowedValue);
            RemoveAllowedValueCommand = new Command<string>(RemoveAllowedValue);
            // Start with one field by default
            Fields.Add(new JobTypeField { Name = "", IsRequired = false });
        }

        private void AddField()
        {
            Fields.Add(new JobTypeField { Name = "", IsRequired = false, FieldType = JobFieldType.Text });
        }

        private void RemoveField(JobTypeField field)
        {
            if (Fields.Contains(field))
                Fields.Remove(field);
        }

        private void AddAllowedValue(JobTypeField field)
        {
            // This will be handled in the code-behind to get the text from the Entry
        }

        private void RemoveAllowedValue(string value)
        {
            // Find the field that contains this value and remove it
            foreach (var field in Fields)
            {
                if (field.AllowedValues.Contains(value))
                {
                    field.AllowedValues.Remove(value);
                    break;
                }
            }
        }

        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(JobTypeName))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Job type name is required.", "OK");
                return;
            }
            if (Fields.Count == 0 || Fields.Any(f => string.IsNullOrWhiteSpace(f.Name)))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "All fields must have a name.", "OK");
                return;
            }
            
            // Validate dropdown fields have allowed values
            var dropdownFieldsWithoutValues = Fields.Where(f => f.FieldType == JobFieldType.Dropdown && f.AllowedValues.Count == 0).ToList();
            if (dropdownFieldsWithoutValues.Any())
            {
                var fieldNames = string.Join(", ", dropdownFieldsWithoutValues.Select(f => f.Name));
                await Application.Current.MainPage.DisplayAlert("Validation Error", $"Dropdown fields must have at least one allowed value: {fieldNames}", "OK");
                return;
            }
            
            // Send new job type to listeners
            var jobType = new JobType
            {
                Name = JobTypeName,
                Fields = Fields.Select(f => new JobTypeField 
                { 
                    Name = f.Name, 
                    IsRequired = f.IsRequired, 
                    FieldType = f.FieldType,
                    AllowedValues = f.AllowedValues.ToList()
                }).ToList()
            };
            WeakReferenceMessenger.Default.Send(new JobTypeCreatedMessage(jobType));
            await Application.Current.MainPage.DisplayAlert("Saved", $"Job type '{JobTypeName}' saved with {Fields.Count} fields.", "OK");
            await Shell.Current.GoToAsync("..", true);
        }
    }

    public class JobTypeCreatedMessage
    {
        public JobTypeCreatedMessage(JobType jobType) => JobType = jobType;
        public JobType JobType { get; }
    }
} 