namespace MassTransit
{
    using System;
    using Confluent.Kafka;
    using DependencyInjection;
    using KafkaIntegration;
    using KafkaIntegration.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;


    public static class KafkaIntegrationExtensions
    {
        public static void UsingKafka(this IRiderRegistrationConfigurator configurator, Action<IRiderRegistrationContext, IKafkaFactoryConfigurator> configure)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRiderFactory(new KafkaRegistrationRiderFactory(configure));
            RegisterComponents(configurator);
        }

        public static void UsingKafka(this IRiderRegistrationConfigurator configurator, ClientConfig clientConfig,
            Action<IRiderRegistrationContext, IKafkaFactoryConfigurator> configure)
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (clientConfig == null)
                throw new ArgumentNullException(nameof(clientConfig));

            configurator.SetRiderFactory(new KafkaRegistrationRiderFactory(clientConfig, configure));
            RegisterComponents(configurator);
        }

        public static void UsingKafka<TBus>(this IRiderRegistrationConfigurator<TBus> configurator,
            Action<IRiderRegistrationContext, IKafkaFactoryConfigurator> configure)
            where TBus : class, IBus
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));

            configurator.SetRiderFactory(new KafkaRegistrationRiderFactory(configure));
            RegisterComponents<TBus>(configurator);
        }

        public static void UsingKafka<TBus>(this IRiderRegistrationConfigurator<TBus> configurator, ClientConfig clientConfig,
            Action<IRiderRegistrationContext, IKafkaFactoryConfigurator> configure)
            where TBus : class, IBus
        {
            if (configurator == null)
                throw new ArgumentNullException(nameof(configurator));
            if (clientConfig == null)
                throw new ArgumentNullException(nameof(clientConfig));

            configurator.SetRiderFactory(new KafkaRegistrationRiderFactory(clientConfig, configure));
            RegisterComponents<TBus>(configurator);
        }

        static void RegisterComponents<TBus>(IRiderRegistrationConfigurator configurator)
        {
            configurator.TryAddScoped<IKafkaRider, Bind<TBus, ITopicProducerProvider>>(GetProducerProvider<TBus>);
        }

        static void RegisterComponents(IRiderRegistrationConfigurator configurator)
        {
            RegisterComponents<IBus>(configurator);
            configurator.TryAddScoped(provider => provider.GetRequiredService<Bind<IBus, ITopicProducerProvider>>().Value);
        }

        static ITopicProducerProvider GetProducerProvider(ITopicProducerProvider producerProvider, IServiceProvider provider)
        {
            var contextProvider = provider.GetService<ScopedConsumeContextProvider>();
            return contextProvider is { HasContext: true }
                ? new ConsumeContextTopicProducerProvider(producerProvider, contextProvider.GetContext())
                : producerProvider;
        }

        static Bind<TBus, ITopicProducerProvider> GetProducerProvider<TBus>(ITopicProducerProvider rider, IServiceProvider provider)
        {
            return Bind<TBus>.Create(GetProducerProvider(rider, provider));
        }
    }
}
