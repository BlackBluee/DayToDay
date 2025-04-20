using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace daytoday.Models
{

    public class UserTask
    {
        public string Title { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }

        public string Priority { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public DateTime Due { get; set; }
        public DateTime Updated { get; set; } = DateTime.Now;

        public DateTime Completed { get; set; } = DateTime.Now;
        public string Time { get; set; }
    }
}
