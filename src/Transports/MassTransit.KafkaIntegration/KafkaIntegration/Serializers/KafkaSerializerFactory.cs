namespace MassTransit.KafkaIntegration.Serializers
{
    using System;
    using System.Collections.Generic;
    using Confluent.Kafka;


    public abstract class KafkaSerializerFactory :
        IKafkaSerializerFactory
    {
        static readonly Dictionary<Type, object> _defaultSerializers = new Dictionary<Type, object>
        {
            [typeof(Null)] = Serializers.Null,
            [typeof(int)] = Serializers.Int32,
            [typeof(long)] = Serializers.Int64,
            [typeof(string)] = Serializers.Utf8,
            [typeof(float)] = Serializers.Single,
            [typeof(double)] = Serializers.Double,
            [typeof(byte[])] = Serializers.ByteArray
        };

        static readonly Dictionary<Type, object> _defaultDeserializers = new Dictionary<Type, object>
        {
            [typeof(Null)] = Deserializers.Null,
            [typeof(Ignore)] = Deserializers.Ignore,
            [typeof(int)] = Deserializers.Int32,
            [typeof(long)] = Deserializers.Int64,
            [typeof(string)] = Deserializers.Utf8,
            [typeof(float)] = Deserializers.Single,
            [typeof(double)] = Deserializers.Double,
            [typeof(byte[])] = Deserializers.ByteArray
        };

        public virtual IAsyncSerializer<T> GetKeySerializer<T>()
        {
            return _defaultSerializers.TryGetValue(typeof(T), out var deserializer)
                ? ((ISerializer<T>)deserializer).AsAsyncOverSync()
                : null;
        }

        public virtual IDeserializer<T> GetKeyDeserializer<T>()
        {
            return _defaultDeserializers.TryGetValue(typeof(T), out var deserializer)
                ? (IDeserializer<T>)deserializer
                : null;
        }

        public abstract IAsyncSerializer<T> GetValueSerializer<T>()
            where T : class;

        public abstract IDeserializer<T> GetValueDeserializer<T>()
            where T : class;
    }
}
