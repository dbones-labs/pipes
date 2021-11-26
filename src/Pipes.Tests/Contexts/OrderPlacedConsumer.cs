namespace Pipes.Tests.Contexts
{
    using System.Threading.Tasks;
    using Infrastructure;

    public class OrderPlacedConsumer : IConsumer<OrderPlaced>
    {
        private readonly Monitor _monitor;
        private readonly Injected _injected;

        public OrderPlacedConsumer(Monitor monitor, Injected injected)
        {
            _monitor = monitor;
            _injected = injected;
        }

        public Task Handle(OrderPlaced message)
        {
            _monitor.AddCallTick(this);
            _injected.Value = message.CustomerId;
            return Task.CompletedTask;
        }
    }
}