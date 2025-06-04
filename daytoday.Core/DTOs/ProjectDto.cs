using daytoday.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daytoday.Core.DTOs
{
    public class ProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string GitHubUrl { get; set; }

        public ICollection<UserTask> UserTasks { get; set; } = new List<UserTask>();

        public ICollection<CalendarEvent> CalendarEvent { get; set; } = new List<CalendarEvent>();
    }
}
