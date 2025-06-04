using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daytoday.Core.Models
{
    public class Project
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string GitHubUrl { get; set; } = string.Empty;
        public ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();

        public ICollection<CalendarEvent> CalendarEvent { get; set; } = new List<CalendarEvent>();


        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
