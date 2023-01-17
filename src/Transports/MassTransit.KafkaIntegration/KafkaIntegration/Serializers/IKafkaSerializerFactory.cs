namespace MassTransit.KafkaIntegration.Serializers
{
    using Confluent.Kafka;


    public interface IKafkaSerializerFactory
    {
        IAsyncSerializer<T> GetKeySerializer<T>();

        IAsyncSerializer<T> GetValueSerializer<T>()
            where T : class;

        IDeserializer<T> GetKeyDeserializer<T>();

        IDeserializer<T> GetValueDeserializer<T>()
            where T : class;
    }
}
