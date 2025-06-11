using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

public class UpdateProjectCommand : IRequest<ProjectDto>
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public string GitHubUrl { get; set; }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly ApplicationDbContext _context;

    public UpdateProjectCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDto> HandleAsync(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
        if (project == null) throw new KeyNotFoundException("Project not found.");

        project.Name = request.Name;
        project.Description = request.Description;
        project.GitHubUrl = request.GitHubUrl;

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
