using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.calendarEvent
{
    public class UpdateCalendarEventCommand : IRequest<CalendarEventDto>
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class UpdateCalendarEventCommandHandler : IRequestHandler<UpdateCalendarEventCommand, CalendarEventDto>
    {
        private readonly ApplicationDbContext _context;
        public UpdateCalendarEventCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CalendarEventDto> HandleAsync(UpdateCalendarEventCommand request, CancellationToken cancellationToken)
        {
            var calendarEvent = await _context.CalendarEvents.FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (calendarEvent == null) throw new KeyNotFoundException("Calendar event not found.");
            calendarEvent.Name = request.Name;
            calendarEvent.Description = request.Description;
            calendarEvent.StartDate = request.StartDate;
            calendarEvent.EndDate = request.EndDate;
            await _context.SaveChangesAsync(cancellationToken);
            return new CalendarEventDto
            {
                Id = calendarEvent.Id,
                Name = calendarEvent.Name,
                Description = calendarEvent.Description,
                StartDate = calendarEvent.StartDate,
                EndDate = calendarEvent.EndDate
            };
        }
    }
}
