using daytoday.Core.Models;
using daytoday.API.Core;
using daytoday.API.Data;

namespace daytoday.API.Mediator.Commands.task
{
    public class CreateTaskCommand : IRequest<Guid>
    {
        public string Title { get; set; }
        public string UserId { get; set; }
    }


    public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, Guid>
    {
        private readonly ApplicationDbContext _context;

        public CreateTaskCommandHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> HandleAsync(CreateTaskCommand request, CancellationToken cancellationToken)
        {
            var task = new UserTask
            {
                Title = request.Title,
                UserId = request.UserId
            };

            _context.UserTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);


            return Guid.NewGuid();
        }
    }
    public class CreateTaskRequest
    {
        public string Title { get; set; }
    }

}
