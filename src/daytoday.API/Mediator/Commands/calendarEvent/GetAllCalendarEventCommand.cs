using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.calendarEvent
{
    public class GetAllCalendarEventCommand : IRequest<List<CalendarEventDto>>
    {
    }

    public class GetAllCalendarEventCommandHandler : IRequestHandler<GetAllCalendarEventCommand, List<CalendarEventDto>>
    {
        private readonly ApplicationDbContext _context;
        public GetAllCalendarEventCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<CalendarEventDto>> HandleAsync(GetAllCalendarEventCommand request, CancellationToken cancellationToken)
        {
            return await _context.CalendarEvents
                .AsNoTracking()
                .Select(e => new CalendarEventDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Description = e.Description,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate
                })
                .ToListAsync(cancellationToken);
        }
    }
}
