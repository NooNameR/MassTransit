namespace MassTransit.MultipleBusRegistration
{
    using System;
    using ExtensionsDependencyInjectionIntegration.MultipleBusRegistration;
    using Metadata;
    using Microsoft.Extensions.DependencyInjection;
    using Registration;


    /// <summary>
    /// Support for multiple bus instances in a single container. This is an advanced concept. Review the documentation
    /// for details on the constraints and known limitations of this approach.
    /// </summary>
    public static class DependencyInjectionMultipleBusRegistrationExtensions
    {
        /// <summary>
        /// Adds the required services to the service collection, and allows consumers to be added and/or discovered
        /// </summary>
        /// <param name="collection">The service collection</param>
        /// <param name="configure">Bus instance configuration method</param>
        public static IServiceCollection AddMassTransit<TBus, TBusInstance>(this IServiceCollection collection,
            Action<IServiceCollectionConfigurator<TBus>> configure)
            where TBus : class, IBusInstance
            where TBusInstance : BusInstance<TBus>, TBus
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var configurator = new ServiceCollectionConfigurator<TBus, TBusInstance>(TypeMetadataCache<TBus>.ShortName, collection);

            configure(configurator);

            return collection;
        }
    }
}
