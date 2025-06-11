using daytoday.API.Core;

namespace daytoday.API.Mediator
{
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<TResponse> Send<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest<TResponse>
        {
            var handler = _serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            var behaviors = _serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse().ToList();

            Task<TResponse> Handler() => handler.HandleAsync(request, cancellationToken);

            return await behaviors.Aggregate((Func<Task<TResponse>>)Handler,
                (next, pipeline) => () => pipeline.Handle(request, cancellationToken, next))();
        }

        public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification
        {
            var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(notification, cancellationToken);
            }
        }
    }
}
