namespace MassTransit.ExtensionsDependencyInjectionIntegration
{
    using System;
    using MassTransit.Registration;
    using Microsoft.Extensions.DependencyInjection;


    public interface IServiceCollectionConfigurator :
        IRegistrationConfigurator<IServiceProvider>
    {
        IServiceCollection Collection { get; }
    }


    public interface IServiceCollectionConfigurator<in TBus> :
        IRegistrationConfigurator<TBus, IServiceProvider>
        where TBus : class, IBusInstance
    {
        IServiceCollection Collection { get; }
    }
}
