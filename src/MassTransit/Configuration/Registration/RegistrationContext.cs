namespace MassTransit.Registration
{
    using Monitoring.Health;


    public class RegistrationContext<TBus, TContainerContext> :
        IRegistrationContext<TBus, TContainerContext>
        where TBus : IBusInstance
        where TContainerContext : class
    {
        readonly IRegistration _registration;
        readonly BusHealth _busHealth;

        public RegistrationContext(Bind<TBus, IRegistration> registration, Bind<TBus, BusHealth> busHealth, TContainerContext container)
        {
            Container = container;

            _registration = registration.Value;
            _busHealth = busHealth.Value;
        }

        public TContainerContext Container { get; }

        public void ConfigureEndpoints<T>(IReceiveConfigurator<T> configurator, IEndpointNameFormatter endpointNameFormatter = null)
            where T : IReceiveEndpointConfigurator
        {
            _registration.ConfigureEndpoints(configurator, endpointNameFormatter);
        }

        public void UseHealthCheck(IBusFactoryConfigurator configurator)
        {
            configurator.ConnectBusObserver(_busHealth);
            configurator.ConnectEndpointConfigurationObserver(_busHealth);
        }
    }
}
