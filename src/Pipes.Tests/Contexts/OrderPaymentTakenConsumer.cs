namespace Pipes.Tests.Contexts
{
    using System.Threading.Tasks;
    using Infrastructure;

    public class OrderPaymentTakenConsumer : IConsumer<OrderPaymentTaken>
    {
        private readonly Monitor _monitor;
        private readonly Injected _injected;

        public OrderPaymentTakenConsumer(Monitor monitor, Injected injected)
        {
            _monitor = monitor;
            _injected = injected;
        }

        public Task Handle(OrderPaymentTaken message)
        {
            _monitor.AddCallTick(this);
            _injected.Value = message.CustomerId;
            return Task.CompletedTask;
        }
    }
}