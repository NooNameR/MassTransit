namespace MassTransit
{
    using Registration;


    public static class HealthCheckConfigurationExtensions
    {
        public static void UseHealthCheck<TBus, TContainerContext>(this IBusFactoryConfigurator configurator,
            IRegistrationContext<TBus, TContainerContext> context)
            where TBus : IBusInstance
            where TContainerContext : class
        {
            context.UseHealthCheck(configurator);
        }
    }
}
