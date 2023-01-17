namespace MassTransit
{
    using System;
    using Confluent.Kafka;
    using KafkaIntegration;


    public static class TopicProducerProviderExtensions
    {
        public static ITopicProducer<TValue> GetProducer<TValue>(this ITopicProducerProvider producerProvider, Uri address)
            where TValue : class
        {
            return GetProducer<Null, TValue>(producerProvider, address, context => default);
        }

        public static ITopicProducer<TValue> GetProducer<TKey, TValue>(this ITopicProducerProvider producerProvider, Uri address,
            KafkaKeyResolver<TKey, TValue> keyResolver)
            where TValue : class
        {
            ITopicProducer<TKey, TValue> producer = producerProvider.GetProducer<TKey, TValue>(address);
            return new KeyedTopicProducer<TKey, TValue>(producer, keyResolver);
        }
    }
}
