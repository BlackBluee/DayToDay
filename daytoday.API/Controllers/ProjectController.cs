using daytoday.API.Core;
using daytoday.API.Mediator.Commands.project;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using daytoday.Core.DTOs;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> GetProjectsAsync()
        {
            var projects = await _mediator.Send<GetAllProjectCommand, List<ProjectDto>>(new GetAllProjectCommand());
            return Ok(projects);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectCommand command)
        {
            var projectId = await _mediator.Send<CreateProjectCommand, Guid>(command);
            return CreatedAtAction(nameof(GetProjectById), new { Id = projectId }, projectId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectById(Guid id)
        {
            var project = await _mediator.Send<GetProjectCommand, ProjectDto>(new GetProjectCommand { Id = id });
            if (project == null)
            {
                return NotFound($"Projekt o ID {id} nie został znaleziony.");
            }
            return Ok(project);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectCommand command)
        {
            command.Id = id; 
            var updatedProject = await _mediator.Send<UpdateProjectCommand, ProjectDto>(command);
            return Ok(updatedProject);
        }

       

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var deletedProject = await _mediator.Send<DeleteProjectCommand, ProjectDto>(new DeleteProjectCommand { Id = id });
            if (deletedProject == null)
            {
                return NotFound($"Projekt o ID {id} nie został znaleziony.");
            }
            return Ok("Projekt usunięty");
        }
    }
}
