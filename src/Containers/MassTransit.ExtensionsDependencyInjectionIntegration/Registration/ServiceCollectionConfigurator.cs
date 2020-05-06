namespace MassTransit.ExtensionsDependencyInjectionIntegration.Registration
{
    using System;
    using Context;
    using MassTransit.Registration;
    using Mediator;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using ScopeProviders;
    using Scoping;
    using Transports;


    public class ServiceCollectionConfigurator :
        RegistrationConfigurator,
        IServiceCollectionConfigurator
    {
        public ServiceCollectionConfigurator(IServiceCollection collection)
            : this(collection, new DependencyInjectionContainerRegistrar(collection))
        {
            collection.AddSingleton<IRegistrationConfigurator>(this);
            collection.AddSingleton(provider => CreateRegistration(provider.GetRequiredService<IConfigurationServiceProvider>()));
        }

        protected ServiceCollectionConfigurator(IServiceCollection collection, IContainerRegistrar registrar)
            : base(registrar)
        {
            Collection = collection;

            AddMassTransitComponents(collection);
        }

        public IServiceCollection Collection { get; }

        public virtual void AddBus(Func<IServiceProvider, IBusControl> busFactory)
        {
            IBusControl BusFactory(IServiceProvider serviceProvider)
            {
                var provider = serviceProvider.GetRequiredService<IConfigurationServiceProvider>();

                ConfigureLogContext(provider);

                return busFactory(serviceProvider);
            }

            Collection.AddSingleton(BusFactory);
            Collection.AddSingleton<IBus>(provider => provider.GetRequiredService<IBusControl>());
            Collection.AddSingleton(provider => ClientFactoryProvider(provider.GetRequiredService<IConfigurationServiceProvider>(),
                provider.GetRequiredService<IBus>()));
        }

        public void AddMediator(Action<IServiceProvider, IReceiveEndpointConfigurator> configure = null)
        {
            IMediator MediatorFactory(IServiceProvider serviceProvider)
            {
                var provider = serviceProvider.GetRequiredService<IConfigurationServiceProvider>();

                ConfigureLogContext(provider);

                return Bus.Factory.CreateMediator(cfg =>
                {
                    configure?.Invoke(serviceProvider, cfg);

                    ConfigureMediator(cfg, provider);
                });
            }

            Collection.TryAddSingleton(MediatorFactory);
            Collection.AddSingleton<IClientFactory>(provider => provider.GetRequiredService<IMediator>());
        }

        void AddMassTransitComponents(IServiceCollection collection)
        {
            collection.TryAddScoped<ScopedConsumeContextProvider>();
            collection.TryAddScoped(provider => provider.GetRequiredService<ScopedConsumeContextProvider>().GetContext() ?? new MissingConsumeContext());

            Collection.TryAddScoped(GetCurrentSendEndpointProvider);
            Collection.TryAddScoped(GetCurrentPublishEndpoint);

            collection.TryAddSingleton<IConsumerScopeProvider>(provider => new DependencyInjectionConsumerScopeProvider(provider));
            collection.TryAddSingleton<ISagaRepositoryFactory>(provider => new DependencyInjectionSagaRepositoryFactory(provider));
            collection.TryAddSingleton<IConfigurationServiceProvider>(provider => new DependencyInjectionConfigurationServiceProvider(provider));
        }

        static ISendEndpointProvider GetCurrentSendEndpointProvider(IServiceProvider provider)
        {
            return (ISendEndpointProvider)provider.GetService<ScopedConsumeContextProvider>()?.GetContext()
                ?? new ScopedSendEndpointProvider<IServiceProvider>(provider.GetRequiredService<IBus>(), provider);
        }

        static IPublishEndpoint GetCurrentPublishEndpoint(IServiceProvider provider)
        {
            return (IPublishEndpoint)provider.GetService<ScopedConsumeContextProvider>()?.GetContext() ?? new PublishEndpoint(
                new ScopedPublishEndpointProvider<IServiceProvider>(provider.GetRequiredService<IBus>(), provider));
        }
    }
}
