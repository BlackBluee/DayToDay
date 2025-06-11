using Microsoft.AspNetCore.Mvc;
using daytoday.API.Core;
using daytoday.API.Mediator.Commands.task;
using daytoday.Core.DTOs;

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
        public async Task<IActionResult> GetTaskAsync()
        {
            var tasks = await _mediator.Send<GetAllTaskCommand, List<TaskDto>>(new GetAllTaskCommand());
            return Ok(tasks);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
        {
            var taskId = await _mediator.Send<CreateTaskCommand, TaskDto>(command);
            return CreatedAtAction(nameof(GetTaskById), new { id = taskId.Id }, taskId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(Guid id)
        {
            var task = await _mediator.Send<GetTaskCommand, TaskDto>(new GetTaskCommand { Id = id });
            if (task == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }
            return Ok(task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskCommand command)
        {
            command.Id = id;
            var updatedTask = await _mediator.Send<UpdateTaskCommand, TaskDto>(command);
            if (updatedTask == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }
            return Ok(updatedTask);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var deletedTask = await _mediator.Send<DeleteTaskCommand, TaskDto>(new DeleteTaskCommand { Id = id });
            if (deletedTask == null)
            {
                return NotFound($"Task with ID {id} not found.");
            }
            return Ok("Projekt usunięty");
        }
    }
}
