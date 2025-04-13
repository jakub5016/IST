namespace EmailService.Utils
{
    public interface IKafkaConsumer<T>
    {
         Task ConsumerLoopAsync(string topic, CancellationToken stoppingToken);
    }
}
