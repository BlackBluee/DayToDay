using daytoday.API.Core;
using daytoday.API.Data;
using daytoday.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.task
{
    public class DeleteTaskCommand : IRequest<TaskDto>
    {
        public Guid Id { get; set; }
    }
    
    public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, TaskDto>
    {
        private readonly ApplicationDbContext _context;
        public DeleteTaskCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<TaskDto> HandleAsync(DeleteTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.UserTasks
                .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (task == null)
                throw new Exception("Task not found");

            _context.UserTasks.Remove(task);
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
