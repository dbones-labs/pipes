namespace Pipes.Tests.AdvancedTest
{
    using System;
    using System.Threading.Tasks;
    using Contexts;
    using Contexts.Actions;
    using Microsoft.Extensions.DependencyInjection;
    using Pipes;

    public class BusMiddleWare<T> : IMiddleware<T> where T : MessageBase
    {
        private readonly Middleware<T> _internalMiddleware;

        public BusMiddleWare()
        {
            _internalMiddleware = new Middleware<T>();
            _internalMiddleware.Add<TestAction1<T>>();
            _internalMiddleware.Add<TestGenericAction<T>>();
            _internalMiddleware.Add<ConsumerAction<T>>();
        }

        public async Task Execute(IServiceProvider scope, T context)
        {
            using var requestScope = scope.CreateScope();
            await _internalMiddleware.Execute(requestScope.ServiceProvider, context);
        }
    }
}