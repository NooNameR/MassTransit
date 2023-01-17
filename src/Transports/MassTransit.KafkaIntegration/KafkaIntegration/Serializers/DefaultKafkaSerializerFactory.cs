namespace MassTransit.KafkaIntegration.Serializers
{
    using Confluent.Kafka;


    public class DefaultKafkaSerializerFactory :
        KafkaSerializerFactory
    {
        public override IAsyncSerializer<T> GetKeySerializer<T>()
        {
            return base.GetKeySerializer<T>() ?? new MassTransitAsyncJsonSerializer<T>();
        }

        public override IDeserializer<T> GetKeyDeserializer<T>()
        {
            return base.GetKeyDeserializer<T>() ?? new MassTransitJsonDeserializer<T>();
        }

        public override IAsyncSerializer<T> GetValueSerializer<T>()
        {
            return new MassTransitAsyncJsonSerializer<T>();
        }

        public override IDeserializer<T> GetValueDeserializer<T>()
        {
            return new MassTransitJsonDeserializer<T>();
        }
    }
}
