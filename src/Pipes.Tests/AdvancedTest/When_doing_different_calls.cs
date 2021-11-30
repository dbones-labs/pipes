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

            _bus.Handle("OrderPlaced", new OrderPlaced
            {
                CustomerId = "asd"
            }).Wait();

        };

        Because of = async () =>
            await _bus.Handle("OrderPaymentTaken", new OrderPaymentTaken
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
}