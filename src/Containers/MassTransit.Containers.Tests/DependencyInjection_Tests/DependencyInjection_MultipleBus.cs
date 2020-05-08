namespace MassTransit.Containers.Tests.DependencyInjection_Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using MultipleBusRegistration;
    using NUnit.Framework;
    using Registration;
    using Scenarios;
    using TestFramework;
    using TestFramework.Logging;
    using TestFramework.Messages;


    [TestFixture]
    public class DependencyInjection_MultipleBus :
        InMemoryTestFixture
    {
        readonly IServiceProvider _provider;
        readonly TaskCompletionSource<ConsumeContext<SimpleMessageInterface>> _task1;
        readonly TaskCompletionSource<ConsumeContext<PingMessage>> _task2;

        public DependencyInjection_MultipleBus()
        {
            _task1 = GetTask<ConsumeContext<SimpleMessageInterface>>();
            _task2 = GetTask<ConsumeContext<PingMessage>>();

            var services = new ServiceCollection();

            services.TryAddSingleton<ILoggerFactory>(provider => new TestOutputLoggerFactory(true));
            services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

            services.AddSingleton(_task1);
            services.AddSingleton(_task2);

            services.AddMassTransit<IBusOne, BusOne>(x =>
            {
                x.AddConsumer<Consumer1>();
                x.AddBus(context => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
                {
                    cfg.Host(new Uri("loopback://bus-one/"));
                    cfg.ConfigureEndpoints(context);
                    cfg.UseHealthCheck(context);
                }));
            });

            services.AddMassTransit<IBusTwo, BusTwo>(x =>
            {
                x.AddConsumer<Consumer2>();
                x.AddBus(context => MassTransit.Bus.Factory.CreateUsingInMemory(cfg =>
                {
                    cfg.Host(new Uri("loopback://bus-two/"));

                    cfg.ConfigureEndpoints(context);
                    cfg.UseHealthCheck(context);
                }));
            });

            _provider = services.BuildServiceProvider(true);
        }

        [OneTimeSetUp]
        public async Task Setup()
        {
            await _provider.GetRequiredService<IBusRegistry>().Start(TestCancellationToken);
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await _provider.GetRequiredService<IBusRegistry>().Stop(TestTimeout);
        }

        [Test]
        public async Task Should_receive()
        {
            var publishEndpoint = _provider.GetService<IBusOne>();
            await publishEndpoint.Publish<SimpleMessageInterface>(new SimpleMessageClass("abc"));

            await _task1.Task;
            await _task2.Task;
        }


        class Consumer1 :
            IConsumer<SimpleMessageInterface>
        {
            readonly IPublishEndpoint _publishEndpoint;
            readonly TaskCompletionSource<ConsumeContext<SimpleMessageInterface>> _taskCompletionSource;

            public Consumer1(IPublishEndpoint publishEndpointDefault, IBusTwo publishEndpoint,
                TaskCompletionSource<ConsumeContext<SimpleMessageInterface>> taskCompletionSource)
            {
                _publishEndpoint = publishEndpoint;
                _taskCompletionSource = taskCompletionSource;
            }

            public async Task Consume(ConsumeContext<SimpleMessageInterface> context)
            {
                _taskCompletionSource.TrySetResult(context);
                await _publishEndpoint.Publish(new PingMessage());
            }
        }


        class Consumer2 :
            IConsumer<PingMessage>
        {
            readonly TaskCompletionSource<ConsumeContext<PingMessage>> _taskCompletionSource;

            public Consumer2(IPublishEndpoint publishEndpoint, TaskCompletionSource<ConsumeContext<PingMessage>> taskCompletionSource)
            {
                _taskCompletionSource = taskCompletionSource;
            }

            public async Task Consume(ConsumeContext<PingMessage> context)
            {
                _taskCompletionSource.TrySetResult(context);
            }
        }


        public interface IBusOne :
            IBusInstance
        {
        }


        public class BusOne :
            BusInstance<IBusOne>,
            IBusOne
        {
            public BusOne(IBusControl busControl)
                : base(busControl)
            {
            }
        }


        public interface IBusTwo :
            IBusInstance
        {
        }


        public class BusTwo :
            BusInstance<IBusTwo>,
            IBusTwo
        {
            public BusTwo(IBusControl busControl)
                : base(busControl)
            {
            }
        }
    }
}
