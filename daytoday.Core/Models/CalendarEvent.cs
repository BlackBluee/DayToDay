using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daytoday.Core.Models
{
    public class CalendarEvent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsAllDay { get; set; } = false;
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
