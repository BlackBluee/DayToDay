using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.calendarEvent
{
    public class GetCalendarEventCommand : IRequest<CalendarEventDto>
    {
        public Guid Id { get; set; }
    }
    
    public class GetCalendarEventCommandHandler : IRequestHandler<GetCalendarEventCommand, CalendarEventDto>
    {
        private readonly ApplicationDbContext _context;
        public GetCalendarEventCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CalendarEventDto> HandleAsync(GetCalendarEventCommand request, CancellationToken cancellationToken)
        {
            var calendarEvent = await _context.CalendarEvents
                .AsNoTracking()
                .Where(e => e.Id == request.Id)
                .Select(e => new CalendarEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    IsAllDay = e.IsAllDay
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (calendarEvent == null)
                throw new Exception("Calendar event not found");
            return calendarEvent;
        }
    }
}
