namespace MassTransit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AspNetCoreIntegration;
    using AspNetCoreIntegration.HealthChecks;
    using Internals.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Monitoring.Health;
    using Registration;


    /// <summary>
    /// These are the updated extensions compatible with the container registration code. They should be used, for real.
    /// </summary>
    public static class HostedServiceConfigurationExtensions
    {
        /// <summary>
        /// Adds the MassTransit <see cref="IHostedService"/>, which includes a bus and endpoint health check.
        /// Use it together with UseHealthCheck to get more detailed diagnostics.
        /// </summary>
        /// <param name="services"></param>
        public static IServiceCollection AddMassTransitHostedService(this IServiceCollection services)
        {
            AddHealthChecks(services);

            return services.AddSingleton<IHostedService, MassTransitHostedService>();
        }

        public static IServiceCollection AddMassTransitHostedService(this IServiceCollection services, IBusControl bus)
        {
            AddHealthChecks(services);

            return services.AddSingleton<IHostedService>(p => new BusHostedService(bus));
        }

        static void AddHealthChecks(IServiceCollection services)
        {
            var builder = services.AddHealthChecks();

            var tags = new[] {"ready"};

            if (services.Any(x => x.ServiceType == typeof(IBusHealth)))
                builder.AddCheck<BusHealthCheck>("bus", HealthStatus.Unhealthy, tags);

            foreach (var descriptor in services)
            {
                if (descriptor.ServiceType != null
                    && descriptor.ServiceType.ClosesType(typeof(Bind<,>), out Type[] types)
                    && types[1] == typeof(BusHealth))
                {
                    var healthCheckType = typeof(BusHealthCheck<>).MakeGenericType(types[0]);

                    builder.Add(new HealthCheckRegistration($"bus-{types[0]}", s => (IHealthCheck)ActivatorUtilities.CreateInstance(s, healthCheckType),
                        HealthStatus.Unhealthy, tags));
                }
            }
        }
    }
}
