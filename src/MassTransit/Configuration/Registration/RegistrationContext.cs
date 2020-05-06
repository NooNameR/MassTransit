namespace MassTransit.Registration
{
    public class RegistrationContext<TBus, TContainerContext> :
        IRegistrationContext<TBus, TContainerContext>
        where TBus : IBusInstance
        where TContainerContext : class
    {
        readonly IRegistration _registration;

        public RegistrationContext(Bind<TBus, IRegistration> registration, TContainerContext container)
        {
            Container = container;
            _registration = registration.Value;
        }

        public TContainerContext Container { get; }

        public void ConfigureEndpoints<T>(IReceiveConfigurator<T> configurator, IEndpointNameFormatter endpointNameFormatter = null)
            where T : IReceiveEndpointConfigurator
        {
            _registration.ConfigureEndpoints(configurator, endpointNameFormatter);
        }
    }
}
