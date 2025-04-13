using Confluent.Kafka;
using EmailService.Configuration;
using EmailService.Consumer;
using EmailService.Events;
using MassTransit.Mediator;
using Microsoft.Extensions.Options;

namespace EmailService.Utils
{
    public class KafkaConsumer<T>: IKafkaConsumer<T>
    {
        private readonly IMediator _mediator;
        private readonly KafkaOptions _kafkaOptions;

        public KafkaConsumer(IOptions<KafkaOptions> kafkaOptions, IMediator mediator)
        {
            _kafkaOptions = kafkaOptions.Value;
            _mediator = mediator;
        }
            public async Task ConsumerLoopAsync(string topic, CancellationToken stoppingToken)
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = _kafkaOptions.ServerAddress,
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    GroupId = "group"
                };

                var consumer = new ConsumerBuilder<Ignore, T>(config)
                    .SetValueDeserializer(new JsonDeserializer<T>())
                    .Build();

                consumer.Subscribe(topic);

                while (!stoppingToken.IsCancellationRequested)
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    if (consumeResult.Message.Value != null)
                    {
                        await _mediator.Send(consumeResult.Message.Value);
                    }
                }

            }

      
    }
}
