using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.project
{
    public class GetAllProjectCommand : IRequest<List<ProjectDto>>
    {
    }

    public class GetAllProjectCommandHandler : IRequestHandler<GetAllProjectCommand, List<ProjectDto>>
    {
        private readonly ApplicationDbContext _context;

        public GetAllProjectCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectDto>> HandleAsync(GetAllProjectCommand request, CancellationToken cancellationToken)
        {
            return await _context.Projects
                .AsNoTracking()
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    GitHubUrl  = p.GitHubUrl
                })
                .ToListAsync(cancellationToken);
        }
    }
}
