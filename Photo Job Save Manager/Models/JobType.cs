using System.Collections.Generic;

namespace Photo_Job_Save_Manager.Models
{
    public class JobType
    {
        public string Name { get; set; } = string.Empty;
        public List<JobTypeField> Fields { get; set; } = new();
    }
} 