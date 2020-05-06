namespace MassTransit.ExtensionsDependencyInjectionIntegration.MultipleBusRegistration
{
    using System;
    using MassTransit.Registration;


    public interface IServiceCollectionConfigurator<in TBus> :
        IRegistrationConfigurator<TBus, IServiceProvider>
        where TBus : class, IBusInstance
    {
    }
}
