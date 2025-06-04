using daytoday.API.Core;
using daytoday.API.Data;
using daytoday.API.Mediator.Commands.project;
using daytoday.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.project
{
    public class DeleteProjectCommand : IRequest<ProjectDto>
    {
        public Guid Id { get; set; }
    }
    

    }
    public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ProjectDto>
    {
        private readonly ApplicationDbContext _context;

    public DeleteProjectCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

    public async Task<ProjectDto> HandleAsync(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects
            .Include(p => p.UserTasks)
            .Include(p => p.CalendarEvent)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (project == null)
            throw new Exception("Project not found");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(cancellationToken);

        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            GitHubUrl = project.GitHubUrl,
            UserTasks = project.UserTasks,
            CalendarEvent = project.CalendarEvent
        };
    }
    
}
