namespace Pipes.Tests.AdvancedTest
{
    using System;
    using Contexts;
    using Contexts.Actions;
    using Fake3rdPartyFramework;
    using Infrastructure;
    using Machine.Specifications;
    using Microsoft.Extensions.DependencyInjection;
    using PowerAssert;

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
}