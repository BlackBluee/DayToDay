namespace daytoday.Core.Models
{
    public class CalendarEvent
    {
        public Guid Id { get; set; }
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
