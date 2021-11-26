namespace Pipes.Tests.AdvancedTest
{
    using System;
    using System.Linq;
    using Contexts;
    using Contexts.Actions;
    using Fake3rdPartyFramework;
    using Infrastructure;
    using Machine.Specifications;
    using Microsoft.Extensions.DependencyInjection;
    using PowerAssert;

    [Subject("fakebus.middleware")]
    public class When_doing_a_single_call
    {
        static IServiceProvider _provider;
        static FakeBus _bus;
        static Injected _injected;
        static Monitor _monitor;

        Establish context = () =>
        {
            _provider = ContainerSetup.BuildWithFakeBus();
            _bus = _provider.GetService<FakeBus>();
            _injected = _provider.GetService<Injected>();
            _monitor = _provider.GetService<Monitor>();

        };

        Because of = async() =>
            await _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd"
            });
        
        It should_have_set_injected_value = () => 
            PAssert.IsTrue(() => _injected.Value == "asd");

        It should_use_the_correct_middleware = () =>
            PAssert.IsTrue(() => _injected.MiddlewareOn == typeof(OrderPlaced));

        It should_create_transient_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCtorTicksFor<TestGenericAction<OrderPlaced>>() == 1);

        It should_dispose_transient_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfDisposalTicksFor<TestGenericAction<OrderPlaced>>() == 1);

        It should_have_invoked_the_generic_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCallTicksFor<TestGenericAction<OrderPlaced>>() == 1);

        It should_have_invoked_the_action1_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCallTicksFor<TestAction1<OrderPlaced>>() == 1);

        It should_reused_singleton_action = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCtorTicksFor<TestAction1<OrderPlaced>>() == 1);

        It should_not_have_disposed_singleton_action = () =>
            PAssert.IsTrue(() => _monitor.NumberOfDisposalTicksFor<TestAction1<OrderPlaced>>() == 0);
    }


    [Subject("fakebus.middleware")]
    public class When_doing_multiple_calls
    {
        static IServiceProvider _provider;
        static FakeBus _bus;
        static Injected _injected;
        static Monitor _monitor;

        Establish context = () =>
        {
            _provider = ContainerSetup.BuildWithFakeBus();
            _bus = _provider.GetService<FakeBus>();
            _injected = _provider.GetService<Injected>();
            _monitor = _provider.GetService<Monitor>();

            _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd"
            }).Wait();

        };

        Because of = async () =>
            await _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd2"
            });

        It should_have_set_injected_value = () =>
            PAssert.IsTrue(() => _injected.Value == "asd2");

        It should_use_the_correct_middleware = () =>
            PAssert.IsTrue(() => _injected.MiddlewareOn == typeof(OrderPlaced));

        It should_create_transient_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCtorTicksFor<TestGenericAction<OrderPlaced>>() == 2);

        It should_dispose_transient_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfDisposalTicksFor<TestGenericAction<OrderPlaced>>() == 2);

        It should_have_invoked_the_generic_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCallTicksFor<TestGenericAction<OrderPlaced>>() == 2);

        It should_have_invoked_the_action1_middleware = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCallTicksFor<TestAction1<OrderPlaced>>() == 2);

        It should_reused_singleton_action = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCtorTicksFor<TestAction1<OrderPlaced>>() == 1);

        It should_not_have_disposed_singleton_action = () =>
            PAssert.IsTrue(() => _monitor.NumberOfDisposalTicksFor<TestAction1<OrderPlaced>>() == 0);
    }


    [Subject("fakebus.middleware")]
    public class When_doing_different_calls
    {
        static IServiceProvider _provider;
        static FakeBus _bus;
        static Injected _injected;
        static Monitor _monitor;

        Establish context = () =>
        {
            _provider = ContainerSetup.BuildWithFakeBus();
            _bus = _provider.GetService<FakeBus>();
            _injected = _provider.GetService<Injected>();
            _monitor = _provider.GetService<Monitor>();

            _bus.Handle("OrderPlaced", new OrderPlaced()
            {
                CustomerId = "asd"
            }).Wait();

        };

        Because of = async () =>
            await _bus.Handle("OrderPaymentTaken", new OrderPaymentTaken()
            {
                CustomerId = "asd2"
            });

        It should_have_set_injected_value = () =>
            PAssert.IsTrue(() => _injected.Value == "asd2");

        It should_use_the_correct_middleware = () =>
            PAssert.IsTrue(() => _injected.MiddlewareOn == typeof(OrderPaymentTaken));

        It should_create_transient_middleware_for_first_message = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCtorTicksFor<TestGenericAction<OrderPlaced>>() == 1);

        It should_create_transient_middleware_for_second_message = () =>
            PAssert.IsTrue(() => _monitor.NumberOfCtorTicksFor<TestGenericAction<OrderPaymentTaken>>() == 1);

    }




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