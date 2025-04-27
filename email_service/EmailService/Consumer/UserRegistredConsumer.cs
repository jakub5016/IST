
using Confluent.Kafka;
using EmailService.Configuration;
using EmailService.Events;
using EmailService.Utils;
using MassTransit.Mediator;
using Microsoft.Extensions.Options;

namespace EmailService.Consumer
{
    public class UserRegistredConsumer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly KafkaOptions _options;
        public UserRegistredConsumer(IServiceProvider serviceProvider, IOptions<KafkaOptions> options)
        {
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<IKafkaConsumer<UserRegistredEvent>>();

            await Task.Factory.StartNew
                (
                    async () => await consumer.ConsumerLoopAsync(_options.UserRegistredTopic, stoppingToken),
                    TaskCreationOptions.LongRunning
                );
        }
       
    }
}
