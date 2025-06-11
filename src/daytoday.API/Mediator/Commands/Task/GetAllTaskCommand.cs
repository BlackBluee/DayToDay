using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.task
{
    public class GetAllTaskCommand : IRequest<List<TaskDto>>
    {
    }

    public class GetAllTaskCommandHandler : IRequestHandler<GetAllTaskCommand, List<TaskDto>>
    {
        private readonly ApplicationDbContext _context;
        public GetAllTaskCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<TaskDto>> HandleAsync(GetAllTaskCommand request, CancellationToken cancellationToken)
        {
            return await _context.UserTasks
                .AsNoTracking()
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Category = t.Category,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority
                })
                .ToListAsync(cancellationToken);
        }
    }
}
