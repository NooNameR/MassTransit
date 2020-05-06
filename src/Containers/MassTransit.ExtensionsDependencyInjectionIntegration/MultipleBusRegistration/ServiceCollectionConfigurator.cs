namespace MassTransit.ExtensionsDependencyInjectionIntegration.MultipleBusRegistration
{
    using System;
    using MassTransit.Registration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Registration;
    using Scoping;
    using Transports;


    public class ServiceCollectionConfigurator<TBus, TBusInstance> :
        ServiceCollectionConfigurator,
        IServiceCollectionConfigurator<TBus>
        where TBus : class, IBusInstance
        where TBusInstance : BusInstance<TBus>, TBus
    {
        public ServiceCollectionConfigurator(string name, IServiceCollection collection)
            : base(collection, new DependencyInjectionContainerRegistrar<TBus>(collection))
        {
            Collection.AddSingleton(provider => new Bind<TBus, IRegistrationConfigurator>(this));
            Collection.AddSingleton(provider => new Bind<TBus, IRegistration>(CreateRegistration(provider.GetRequiredService<IConfigurationServiceProvider>())));

            Collection.TryAddSingleton<IBusRegistry, BusRegistry>();
        }

        public void AddBus(Func<IRegistrationContext<TBus, IServiceProvider>, IBusControl> busFactory)
        {
            IBusControl BusFactory(IServiceProvider serviceProvider)
            {
                var provider = serviceProvider.GetRequiredService<IConfigurationServiceProvider>();

                ConfigureLogContext(provider);

                var context = serviceProvider.GetRequiredService<IRegistrationContext<TBus, IServiceProvider>>();

                return busFactory(context);
            }

            Collection.AddSingleton<IRegistrationContext<TBus, IServiceProvider>>(provider =>
                new RegistrationContext<TBus, IServiceProvider>(provider.GetRequiredService<Bind<TBus, IRegistration>>(), provider));

            Collection.AddSingleton(provider => new Bind<TBus, IBusControl>(BusFactory(provider)));

            Collection.AddSingleton<TBus>(provider => ActivatorUtilities.CreateInstance<TBusInstance>(provider,
                provider.GetRequiredService<Bind<TBus, IBusControl>>().Value));

            Collection.AddSingleton(provider => new Bind<TBus, IBus>(provider.GetRequiredService<TBus>()));
            Collection.AddSingleton(provider => new Bind<TBus, ISendEndpointProvider>(GetSendEndpointProvider(provider)));
            Collection.AddSingleton(provider => new Bind<TBus, IPublishEndpoint>(GetPublishEndpoint(provider)));
            Collection.AddSingleton(provider => new Bind<TBus, IClientFactory>(ClientFactoryProvider(
                provider.GetRequiredService<IConfigurationServiceProvider>(), provider.GetRequiredService<TBus>())));

            Collection.AddSingleton<IBusRegistryInstance>(provider => provider.GetRequiredService<BusRegistryInstance<TBus>>());

            Collection.AddScoped(GetSendEndpointProvider);
            Collection.AddScoped(GetPublishEndpoint);
        }

        static ISendEndpointProvider GetSendEndpointProvider(IServiceProvider provider)
        {
            return new ScopedSendEndpointProvider<IServiceProvider>(provider.GetRequiredService<TBus>(), provider);
        }

        static IPublishEndpoint GetPublishEndpoint(IServiceProvider provider)
        {
            return new PublishEndpoint(new ScopedPublishEndpointProvider<IServiceProvider>(provider.GetRequiredService<TBus>(), provider));
        }
    }
}
