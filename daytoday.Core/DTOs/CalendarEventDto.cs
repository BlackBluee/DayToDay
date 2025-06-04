using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daytoday.Core.DTOs
{
    public class CalendarEventDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } 
        public string Location { get; set; } 
        public bool IsAllDay { get; set; } 
    }
}
