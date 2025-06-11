using daytoday.API.Core;

using daytoday.Core.DTOs;
using daytoday.API.Data;
using daytoday.Core.Models;
using System.IdentityModel.Tokens.Jwt;

namespace daytoday.API.Mediator.Commands.calendarEvent
{
    
public class CreateCalendarEventCommand : IRequest<CalendarEventDto>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public bool IsAllDay { get; set; }
    }

    public class CreateCreateCalendarEventCommandHandler : IRequestHandler<CreateCalendarEventCommand, CalendarEventDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateCreateCalendarEventCommandHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<CalendarEventDto> HandleAsync(CreateCalendarEventCommand request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Brak tokenu w nagłówku.");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Brak ID użytkownika w tokenie.");

            var calendarEvent = new CalendarEvent
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location ?? string.Empty,
                IsAllDay = request.IsAllDay,
                UserId = userId
            };

            _context.CalendarEvents.Add(calendarEvent);
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
    public class CreateCalendarEventCommandHandler : IRequestHandler<CreateCalendarEventCommand, CalendarEventDto>
    {
        public async Task<CalendarEventDto> HandleAsync(CreateCalendarEventCommand request, CancellationToken cancellationToken)
        {
            // Implement the logic to handle the command and return a CalendarEventDto  
            var calendarEvent = new CalendarEventDto
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Location = request.Location,
                IsAllDay = request.IsAllDay
            };

            return await Task.FromResult(calendarEvent);
        }
    }
}
