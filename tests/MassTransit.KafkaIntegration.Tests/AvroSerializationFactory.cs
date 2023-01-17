namespace MassTransit.KafkaIntegration.Tests
{
    using Confluent.Kafka;
    using Confluent.Kafka.SyncOverAsync;
    using Confluent.SchemaRegistry;
    using Confluent.SchemaRegistry.Serdes;
    using Serializers;


    public class AvroSerializationFactory :
        KafkaSerializerFactory
    {
        readonly AvroDeserializerConfig _deserializerConfig;
        readonly ISchemaRegistryClient _schemaRegistryClient;
        readonly AvroSerializerConfig _serializerConfig;

        public AvroSerializationFactory(ISchemaRegistryClient schemaRegistryClient, AvroSerializerConfig serializerConfig = null,
            AvroDeserializerConfig deserializerConfig = null)
        {
            _schemaRegistryClient = schemaRegistryClient;
            _serializerConfig = serializerConfig;
            _deserializerConfig = deserializerConfig;
        }

        public override IAsyncSerializer<T> GetValueSerializer<T>()
            where T : class
        {
            return new AvroSerializer<T>(_schemaRegistryClient, _serializerConfig);
        }

        public override IDeserializer<T> GetValueDeserializer<T>()
            where T : class
        {
            return new AvroDeserializer<T>(_schemaRegistryClient, _deserializerConfig).AsSyncOverAsync();
        }
    }
}
