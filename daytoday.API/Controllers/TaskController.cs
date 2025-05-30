using Microsoft.AspNetCore.Mvc;
using daytoday.API.Core;
using daytoday.API.Mediator.Commands.task;

namespace daytoday.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TaskController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult GetTasks()
        {
            return Ok("List of tasks will be returned here.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            var command = new CreateTaskCommand
            {
                Title = request.Title
            };

            var result = await _mediator.Send<CreateTaskCommand, Guid>(command);

            return CreatedAtAction(nameof(GetTasks), new { id = result }, result);
        }

        [HttpGet("{id}")]
        public IActionResult GetTaskById(Guid id)
        {
            return Ok($"Task with ID {id} will be returned here.");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateTask(Guid id, [FromBody] CreateTaskRequest request)
        {
            return Ok($"Task with ID {id} will be updated with Title {request.Title}.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteTask(Guid id)
        {
            return Ok($"Task with ID {id} will be deleted.");
        }
    }
}
