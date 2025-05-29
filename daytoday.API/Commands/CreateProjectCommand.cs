using daytoday.Core.Models;
using daytoday.API.Core;
using daytoday.API.Data;

namespace daytoday.API.Commands
{
    public class CreateProjectCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string UserId { get; set; }
    }

    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CreateProjectCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> HandleAsync(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var project = new Project
            {
                Name = request.Name,
                UserId = request.UserId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            
            return Guid.NewGuid(); 
        }
    }
    public class CreateProjectRequest
    {
        public string Name { get; set; }
    }

}
