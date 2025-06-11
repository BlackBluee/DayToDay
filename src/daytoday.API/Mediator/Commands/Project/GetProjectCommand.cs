using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.project
{
    public class GetProjectCommand : IRequest<ProjectDto>
    {
        public Guid Id { get; set; }
    }

    public class GetProjectCommandHandler : IRequestHandler<GetProjectCommand, ProjectDto>
    {
        private readonly ApplicationDbContext _context;

        public GetProjectCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectDto> HandleAsync(GetProjectCommand request, CancellationToken cancellationToken)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Where(p => p.Id == request.Id)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    GitHubUrl = p.GitHubUrl,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
                throw new Exception("Project not found");

            return project;
        }
    }
}
