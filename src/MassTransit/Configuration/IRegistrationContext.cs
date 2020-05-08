namespace MassTransit
{
    using Registration;


    public interface IRegistrationContext<in TBus, out TContainerContext>
        where TBus : IBusInstance
        where TContainerContext : class
    {
        TContainerContext Container { get; }

        void ConfigureEndpoints<T>(IReceiveConfigurator<T> configurator, IEndpointNameFormatter endpointNameFormatter = null)
            where T : IReceiveEndpointConfigurator;

        void UseHealthCheck(IBusFactoryConfigurator configurator);
    }
}
