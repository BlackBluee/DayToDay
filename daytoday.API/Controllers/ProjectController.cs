using daytoday.API.Commands;
using daytoday.API.Core;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace daytoday.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult GetProjects()
        {
            return Ok("List of projects will be returned here.");
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var command = new CreateProjectCommand
            {
                Name = request.Name
            };

            var result = await _mediator.Send<CreateProjectCommand, Guid>(command);

            return CreatedAtAction(nameof(GetProjects), new { id = result }, result);
        }

        [HttpGet("{id}")]
        public IActionResult GetProjectById(Guid id)
        {
            return Ok($"Project with ID {id} will be returned here.");
        }

        [HttpPut("{id}")]
        public IActionResult UpdateProject(Guid id, [FromBody] CreateProjectRequest request)
        {
            return Ok($"Project with ID {id} will be updated with name {request.Name}.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteProject(Guid id)
        {
            return Ok($"Project with ID {id} will be deleted.");
        }
    }


    }
