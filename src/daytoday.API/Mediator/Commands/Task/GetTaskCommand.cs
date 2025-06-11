using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.task
{
    public class GetTaskCommand : IRequest<TaskDto>
    {
        public Guid Id { get; set; }
    }
    
    public class GetTaskCommandHandler : IRequestHandler<GetTaskCommand, TaskDto>
    {
        private readonly ApplicationDbContext _context;
        public GetTaskCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<TaskDto> HandleAsync(GetTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.UserTasks
                .AsNoTracking()
                .Where(t => t.Id == request.Id)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Category = t.Category,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (task == null)
                throw new Exception("Task not found");
            return task;
        }
    }
}
