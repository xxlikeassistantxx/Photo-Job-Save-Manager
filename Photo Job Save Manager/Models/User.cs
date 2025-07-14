using System.ComponentModel.DataAnnotations;

namespace Photo_Job_Save_Manager.Models
{
    public class User
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public long StorageUsed { get; set; } // in bytes
        public bool IsEmailVerified { get; set; }
    }
} 