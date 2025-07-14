using System;
using System.Collections.Generic;
using Photo_Job_Save_Manager.Models;

namespace Photo_Job_Save_Manager.Models
{
    public class RecycleBinEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } // "JobType" or "Job"
        public JobType? DeletedJobType { get; set; }
        public List<Job>? DeletedJobs { get; set; } // For JobType deletion
        public Job? DeletedJob { get; set; } // For single job deletion
    }
} 