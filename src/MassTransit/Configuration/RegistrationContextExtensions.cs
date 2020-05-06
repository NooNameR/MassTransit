namespace MassTransit
{
    using Definition;
    using Registration;


    public static class RegistrationContextExtensions
    {
        /// <summary>
        /// Configure the endpoints for all defined consumer, saga, and activity types using an optional
        /// endpoint name formatter. If no endpoint name formatter is specified and an <see cref="IEndpointNameFormatter"/>
        /// is registered in the container, it is resolved from the container. Otherwise, the <see cref="DefaultEndpointNameFormatter"/>
        /// is used.
        /// </summary>
        /// <param name="configurator">The <see cref="IBusFactoryConfigurator"/> for the bus being configured</param>
        /// <param name="context"></param>
        /// <param name="endpointNameFormatter">Optional, the endpoint name formatter</param>
        /// <typeparam name="T">The bus factory type (depends upon the transport)</typeparam>
        /// <typeparam name="TContainerContext"></typeparam>
        /// <typeparam name="TBus">The bus instance type</typeparam>
        public static void ConfigureEndpoints<T, TBus, TContainerContext>(this IReceiveConfigurator<T> configurator,
            IRegistrationContext<TBus, TContainerContext> context, IEndpointNameFormatter endpointNameFormatter = null)
            where T : IReceiveEndpointConfigurator
            where TContainerContext : class
            where TBus : IBusInstance
        {
            context.ConfigureEndpoints(configurator, endpointNameFormatter);
        }
    }
}
