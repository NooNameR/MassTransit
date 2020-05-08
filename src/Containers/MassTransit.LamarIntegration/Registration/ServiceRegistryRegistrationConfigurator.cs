namespace MassTransit.LamarIntegration.Registration
{
    using System;
    using Lamar;
    using MassTransit.Registration;
    using Mediator;
    using ScopeProviders;
    using Scoping;
    using Transports;


    public class ServiceRegistryRegistrationConfigurator :
        RegistrationConfigurator,
        IServiceRegistryConfigurator
    {
        readonly ServiceRegistry _registry;

        public ServiceRegistryRegistrationConfigurator(ServiceRegistry registry)
            : base(new LamarContainerRegistrar(registry))
        {
            _registry = registry;

            registry.For<IConsumerScopeProvider>()
                .Use(CreateConsumerScopeProvider)
                .Singleton();

            registry.For<ISagaRepositoryFactory>()
                .Use(CreateSagaRepositoryFactory)
                .Singleton();

            registry.For<IConfigurationServiceProvider>()
                .Use(context => new LamarConfigurationServiceProvider(context.GetInstance<IContainer>()))
                .Singleton();

            registry.For<IRegistrationConfigurator>()
                .Use(this);

            registry.For<IRegistration>()
                .Use(provider => CreateRegistration(provider.GetInstance<IConfigurationServiceProvider>()))
                .Singleton();

            registry.Injectable<ConsumeContext>();
        }

        ServiceRegistry IServiceRegistryConfigurator.Builder => _registry;

        public void AddBus(Func<IServiceContext, IBusControl> busFactory)
        {
            IBusControl BusFactory(IServiceContext context)
            {
                var provider = context.GetInstance<IConfigurationServiceProvider>();

                ConfigureLogContext(provider);

                return busFactory(context);
            }

            _registry.For<IBusControl>()
                .Use(BusFactory)
                .Singleton();

            _registry.For<IBus>()
                .Use(context => context.GetInstance<IBusControl>())
                .Singleton();

            _registry.For<ISendEndpointProvider>()
                .Use(GetCurrentSendEndpointProvider)
                .Scoped();

            _registry.For<IPublishEndpoint>()
                .Use(GetCurrentPublishEndpoint)
                .Scoped();

            _registry.For<IClientFactory>()
                .Use(context => ClientFactoryProvider(context.GetInstance<IConfigurationServiceProvider>(), context.GetInstance<IBus>()))
                .Singleton();
        }

        public void AddMediator(Action<IServiceContext, IReceiveEndpointConfigurator> configure = null)
        {
            IMediator MediatorFactory(IServiceContext context)
            {
                var provider = context.GetInstance<IConfigurationServiceProvider>();

                ConfigureLogContext(provider);

                return Bus.Factory.CreateMediator(cfg =>
                {
                    configure?.Invoke(context, cfg);

                    ConfigureMediator(cfg, provider);
                });
            }

            _registry.For<IMediator>()
                .Use(MediatorFactory)
                .Singleton();

            _registry.For<IClientFactory>()
                .Use(context => context.GetInstance<IMediator>())
                .Singleton();
        }

        static IConsumerScopeProvider CreateConsumerScopeProvider(IServiceContext context)
        {
            return new LamarConsumerScopeProvider(context.GetInstance<IContainer>());
        }

        static ISagaRepositoryFactory CreateSagaRepositoryFactory(IServiceContext context)
        {
            return new LamarSagaRepositoryFactory(context.GetInstance<IContainer>());
        }

        static ISendEndpointProvider GetCurrentSendEndpointProvider(IServiceContext context)
        {
            return (ISendEndpointProvider)context.TryGetInstance<ConsumeContext>()
                ?? new ScopedSendEndpointProvider<IContainer>(context.GetInstance<IBus>(), context.GetInstance<IContainer>());
        }

        static IPublishEndpoint GetCurrentPublishEndpoint(IServiceContext context)
        {
            return (IPublishEndpoint)context.TryGetInstance<ConsumeContext>()
                ?? new PublishEndpoint(new ScopedPublishEndpointProvider<IContainer>(context.GetInstance<IBus>(), context.GetInstance<IContainer>()));
        }
    }
}
