using daytoday.Core.Models;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using daytoday.Core.DTOs;

namespace daytoday.API.Mediator.Commands.task
{
    public class CreateTaskCommand : IRequest<TaskDto>
    {
        public required string Title { get; set; }
        public string? Category { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public Guid? ProjectId { get; set; }
    }


    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateTaskCommandHandler(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TaskDto> HandleAsync(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString()?.Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
                throw new UnauthorizedAccessException("Brak tokenu w nagłówku.");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("Brak ID użytkownika w tokenie.");

            var task = new UserTask
            {
                // Use sequential ID instead of Guid for this entity
                // PostgreSQL is expecting an integer, not a UUID
                // Id field will be automatically set by the database
                Title = request.Title,
                Category = request.Category ?? string.Empty,
                Description = request.Description ?? string.Empty,
                Status = request.Status ?? "Pending",
                Priority = request.Priority ?? "Normal",
                Created = DateTime.UtcNow,
                Due = DateTime.UtcNow.AddDays(7),
                Updated = DateTime.UtcNow,
                UserId = userId,
            };

            _context.UserTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);

            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Category = task.Category,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority
            };
        }
    }
    

}
