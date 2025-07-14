namespace Photo_Job_Save_Manager.Models
{
    public class JobTypeField : ViewModels.BaseViewModel
    {
        public string Name { get; set; } = string.Empty;
        public bool IsRequired { get; set; }
        private JobFieldType _fieldType = JobFieldType.Text;
        public JobFieldType FieldType
        {
            get => _fieldType;
            set => SetProperty(ref _fieldType, value);
        }
        public List<string> AllowedValues { get; set; } = new();
    }

    public enum JobFieldType
    {
        Text,
        Photo,
        Dropdown
    }
} 