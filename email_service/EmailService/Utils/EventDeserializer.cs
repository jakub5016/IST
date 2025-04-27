using Confluent.Kafka;
using EmailService.Events;
using System.Text;
using System.Text.Json;

namespace EmailService.Consumer
{
    public class EventDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            if (isNull || data.IsEmpty)
                return default!;

            var json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<T>(json,JsonSerializerOptions.Web)!;
        }
    }
}

