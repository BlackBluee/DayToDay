using daytoday.API.Core;
using daytoday.API.Data;
using daytoday.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.calendarEvent
{
    public class DeleteCalendarEventCommand : IRequest<CalendarEventDto>
    {
        public Guid Id { get; set; }
    }

    public class DeleteCalendarEventCommandHandler : IRequestHandler<DeleteCalendarEventCommand, CalendarEventDto>
    {
        private readonly ApplicationDbContext _context;


        public DeleteCalendarEventCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CalendarEventDto> HandleAsync(DeleteCalendarEventCommand request, CancellationToken cancellationToken)
        {
            var calendarEvent = await _context.CalendarEvents
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
            if (calendarEvent == null)
                throw new Exception("Calendar event not found");
            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync(cancellationToken);
            return new CalendarEventDto
            {
                Id = calendarEvent.Id,
                Name = calendarEvent.Name,
                Description = calendarEvent.Description,
                StartDate = calendarEvent.StartDate,
                EndDate = calendarEvent.EndDate,
                Location = calendarEvent.Location,
                IsAllDay = calendarEvent.IsAllDay
            };
        }
    }
    }
