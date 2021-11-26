namespace Pipes.Tests.Contexts.Actions
{
    using System.Threading.Tasks;
    using Pipes;

    public class ConsumerAction<T> : IAction<T>
    {
        private readonly IConsumer<T> _consumer;

        public ConsumerAction(IConsumer<T> consumer)
        {
            _consumer = consumer;
        }

        public async Task Execute(T context, Next<T> next)
        {
            await _consumer.Handle(context);
            await next(context);
        }
    }
}