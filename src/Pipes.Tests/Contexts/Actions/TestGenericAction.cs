namespace Pipes.Tests.Contexts.Actions
{
    using System;
    using System.Threading.Tasks;
    using Infrastructure;
    using Pipes;

    public class TestGenericAction<T> : IAction<T>, IDisposable
    {
        private readonly Monitor _monitor;
        private readonly Injected _injected;

        public TestGenericAction(Monitor monitor, Injected injected)
        {
            _monitor = monitor;
            _injected = injected;
            _monitor.AddCtorTick(this);
        }

        public async Task Execute(T context, Next<T> next)
        {
            _monitor.AddCallTick(this);
            await next(context);
            _injected.MiddlewareOn = typeof(T);
        }

        public void Dispose()
        {
            _monitor.AddDisposalTick(this);
        }
    }
}