using System.Collections.Generic;

namespace Photo_Job_Save_Manager.Models
{
    public class Job
    {
        public string Id { get; set; } = string.Empty;
        public string JobTypeId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public List<JobPhoto> Photos { get; set; } = new List<JobPhoto>();

        // Computed property to get the job name from the "Name" field
        public string JobName 
        { 
            get 
            {
                if (Data.TryGetValue("Name", out var name))
                {
                    return name?.ToString() ?? "Unnamed Job";
                }
                return "Unnamed Job";
            }
        }

        // Computed property to get a nicer display name for the job type
        public string JobTypeName 
        { 
            get 
            {
                // This will be set by the ViewModel when loading jobs
                return !string.IsNullOrEmpty(JobTypeId) ? JobTypeId : "Unknown Type";
            }
        }

        // Computed property to get all field data except the "Name" field
        public Dictionary<string, object> OtherFieldData
        {
            get
            {
                var otherData = new Dictionary<string, object>();
                foreach (var kvp in Data)
                {
                    if (kvp.Key != "Name")
                    {
                        otherData[kvp.Key] = kvp.Value;
                    }
                }
                return otherData;
            }
        }
    }

    public class JobPhoto
    {
        public string Id { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string FirebaseUrl { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public long FileSize { get; set; }
    }
} 