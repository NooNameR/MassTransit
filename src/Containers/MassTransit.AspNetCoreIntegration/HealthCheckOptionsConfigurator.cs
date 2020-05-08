namespace MassTransit.AspNetCoreIntegration
{
    using System.Collections.Generic;
    using HealthChecks;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Options;
    using Monitoring.Health;


    public class BusHealthCheckOptionsConfigurator :
        IConfigureOptions<HealthCheckServiceOptions>
    {
        readonly IEnumerable<IBusHealth> _busHealths;
        readonly string[] _tags;

        public BusHealthCheckOptionsConfigurator(IEnumerable<IBusHealth> busHealths, string[] tags)
        {
            _busHealths = busHealths;
            _tags = tags;
        }

        public void Configure(HealthCheckServiceOptions options)
        {
            foreach (var busHealth in _busHealths)
                options.Registrations.Add(new HealthCheckRegistration(busHealth.Name, new BusHealthCheck(busHealth), HealthStatus.Unhealthy, _tags));
        }
    }
}
