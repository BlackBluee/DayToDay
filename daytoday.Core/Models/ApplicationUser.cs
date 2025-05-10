
using Microsoft.AspNetCore.Identity;

namespace daytoday.Core.Models
{
    
        public class ApplicationUser : IdentityUser
        {
            public string DisplayName { get; set; } = string.Empty;

            public ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();
            public ICollection<Project> Projects { get; set; } = new List<Project>();
        }
    
}
