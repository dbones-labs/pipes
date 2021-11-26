namespace Pipes.Tests.AdvancedTest
{
    using System;
    using Contexts;
    using Fake3rdPartyFramework;
    using Microsoft.Extensions.DependencyInjection;

    public static class FakeBusExtensions
    {
        public static void Subscribe<T>(this FakeBus bus, string topic, IServiceProvider provider) where T : MessageBase
        {
            bus.Subscribe<T>(topic, msg =>
            {
                var pipeline = provider.GetService<BusMiddleWare<T>>(); 
                return pipeline.Execute(provider, msg);
            });
        }
    }
}