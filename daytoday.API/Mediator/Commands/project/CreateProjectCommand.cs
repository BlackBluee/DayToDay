using daytoday.Core.Models;
using daytoday.API.Core;
using daytoday.API.Data;
using System.IdentityModel.Tokens.Jwt;

namespace daytoday.API.Mediator.Commands.project
{
    public class CreateProjectCommand : IRequest<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string GitHubUrl { get; set; } = string.Empty;
    }

    public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Guid>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateProjectCommandHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Guid> HandleAsync(CreateProjectCommand request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Brak tokenu w nagłówku.");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Brak ID użytkownika w tokenie.");

            var project = new Project
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                GitHubUrl = request.GitHubUrl ?? string.Empty,
                UserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            return project.Id;
        }
    }
    public class CreateProjectRequest
    {
        public string Name { get; set; }
    }

}
