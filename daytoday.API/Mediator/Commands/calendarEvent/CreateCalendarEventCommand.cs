using daytoday.API.Core;
using daytoday.API.Data;
using daytoday.Core.Models;

namespace daytoday.API.Mediator.Commands.calendarEvent
{
    
    public class CreateCalendarEventCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string UserId { get; set; }
    }

    public class CreateCreateCalendarEventCommandHandler : IRequestHandler<CreateCalendarEventCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CreateCreateCalendarEventCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> HandleAsync(CreateCalendarEventCommand request, CancellationToken cancellationToken)
        {
            var project = new CalendarEvent
            {
                Name = request.Name,
                UserId = request.UserId
            };

            _context.CalendarEvents.Add(project);
            await _context.SaveChangesAsync(cancellationToken);


            return Guid.NewGuid();
        }
    }
    public class CreateCalendarEventRequest
    {
        public string Name { get; set; }
    }
}
