using daytoday.API.Core;
using daytoday.API.Mediator.Commands.calendarEvent;
using daytoday.API.Mediator.Commands.task;
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
        public IActionResult GetCalendarEvent()
        {
            return Ok("List of CalendarEvent will be returned here.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateCalendarEvent([FromBody] CreateCalendarEventRequest request)
        {
            var command = new CreateCalendarEventCommand
            {
                Name = request.Name
            };

            var result = await _mediator.Send<CreateCalendarEventCommand, Guid>(command);

            return CreatedAtAction(nameof(GetCalendarEvent), new { id = result }, result);
        }

        [HttpGet("{id}")]
        public IActionResult GetCalendarEventById(Guid id)
        {
            return Ok($"CalendarEvent with ID {id} will be returned here.");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCalendarEvent(Guid id, [FromBody] CreateCalendarEventRequest request)
        {
            return Ok($"CalendarEvent with ID {id} will be updated with Title {request.Name}.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCalendarEvent(Guid id)
        {
            return Ok($"CalendarEvent with ID {id} will be deleted.");
        }

    }
}
