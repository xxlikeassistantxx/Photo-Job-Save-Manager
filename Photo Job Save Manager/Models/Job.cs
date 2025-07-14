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