using daytoday.Core.DTOs;
using daytoday.API.Core;
using daytoday.API.Data;
using Microsoft.EntityFrameworkCore;

namespace daytoday.API.Mediator.Commands.task
{
    public class UpdateTaskCommand : IRequest<TaskDto>
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public required string Category { get; set; }
        public required string Description { get; set; }
        public required string Status { get; set; }
        public required string Priority { get; set; }
    }

    public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
    {
        private readonly ApplicationDbContext _context;
        public UpdateTaskCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TaskDto> HandleAsync(UpdateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = await _context.UserTasks.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken);
            if (task == null) throw new KeyNotFoundException("Task not found.");

            task.Title = request.Title;
            task.Category = request.Category;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
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
