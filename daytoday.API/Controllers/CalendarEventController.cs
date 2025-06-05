using daytoday.API.Core;
using daytoday.API.Mediator.Commands.calendarEvent;
using daytoday.Core.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace daytoday.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarEventController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CalendarEventController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCalendarEventAsync()
        {
            var calendarEvents = await _mediator.Send<GetAllCalendarEventCommand, List<CalendarEventDto>>(new GetAllCalendarEventCommand());
            return Ok(calendarEvents);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCalendarEvent([FromBody] CreateCalendarEventCommand command)
        {
            var calendarEventId = await _mediator.Send<CreateCalendarEventCommand, CalendarEventDto>(command);
            return CreatedAtAction(nameof(GetCalendarEventById), new { id = calendarEventId }, calendarEventId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCalendarEventById(Guid id)
        {
            var calendarEvent = await _mediator.Send<GetCalendarEventCommand, CalendarEventDto>(new GetCalendarEventCommand { Id = id });
            if (calendarEvent == null)
            {
                return NotFound($"Calendar event with ID {id} not found.");
            }
            return Ok(calendarEvent);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalendarEvent(Guid id, [FromBody] UpdateCalendarEventCommand command)
        {
            command.Id = id; 
            var updatedCalendarEvent = await _mediator.Send<UpdateCalendarEventCommand, CalendarEventDto>(command);
            if (updatedCalendarEvent == null)
            {
                return NotFound($"Calendar event with ID {id} not found.");
            }
            return Ok(updatedCalendarEvent);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalendarEvent(Guid id)
        {
            var deletedCalendarEvent = await _mediator.Send<DeleteCalendarEventCommand, CalendarEventDto>(new DeleteCalendarEventCommand { Id = id });
            if (deletedCalendarEvent == null)
            {
                return NotFound($"Projekt o ID {id} nie został znaleziony.");
            }
            return Ok("Projekt usunięty");
        }

    }
}
