namespace Pipes.Tests.AdvancedTest
{
    using System;
    using System.Linq;
    using Contexts;
    using Contexts.Actions;
    using Fake3rdPartyFramework;
    using Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    public static class ContainerSetup
    {
        public static IServiceProvider Build(Action<IServiceCollection> setup = null)
        {
            var monitor = new Monitor(); 
            var injected = new Injected();
            var collection = new ServiceCollection();

            setup?.Invoke(collection);

            collection.AddSingleton(monitor);
            collection.AddSingleton(injected);

            return collection.BuildServiceProvider();
        }

        public static IServiceProvider BuildWithFakeBus(Action<IServiceCollection> setup = null)
        {
            return Build(collection =>
            {
                //this registration is key, this is where the actions are associated with a middleware implementation.
                collection.AddSingleton(typeof(BusMiddleWare<>), typeof(BusMiddleWare<>));
                //register the Actions.
                collection.AddTransient(typeof(ConsumerAction<>));
                collection.AddSingleton(typeof(TestAction1<>));
                collection.AddTransient(typeof(TestGenericAction<>));


                //find all the IConsumer<T> interfaces
                var consumers = typeof(ContainerSetup).Assembly.ExportedTypes.SelectMany(x =>
                {
                    var interfaces = x.GetInterfaces()
                        .Where(interfaceType => interfaceType.IsGenericType)
                        .Where(interfaceType => typeof(IConsumer<>).IsAssignableFrom(interfaceType.GetGenericTypeDefinition()))
                        .Select(interfaceType => new
                        {
                            Type = x,
                            Interface = interfaceType,
                            ConsumedType = interfaceType.GenericTypeArguments[0]
                        });

                    return interfaces;
                }).ToList();


                foreach (var consumer in consumers)
                {
                    //Register IConsumer<T>
                    collection.AddTransient(consumer.Interface, consumer.Type);
                }



                //the framework we want to add middleware support with.
                collection.AddSingleton<FakeBus>(s =>
                {
                    var bus = new FakeBus();

                    bus.Subscribe<OrderPlaced>("OrderPlaced", s);
                    bus.Subscribe<OrderPaymentTaken>("OrderPaymentTaken", s);

                    return bus;
                });

                setup?.Invoke(collection);

            });
        }
    }
}