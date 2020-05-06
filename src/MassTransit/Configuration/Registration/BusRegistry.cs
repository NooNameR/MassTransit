namespace MassTransit.Registration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;


    public class BusRegistry :
        IBusRegistry
    {
        readonly IDictionary<Type, IBusRegistryInstance> _instances;

        public BusRegistry(IEnumerable<IBusRegistryInstance> instances)
        {
            _instances = instances.ToDictionary(x => x.InstanceType);
        }
    }
}
