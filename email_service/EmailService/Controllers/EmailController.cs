using EmailService.Configuration;
using EmailService.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EmailService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController(ITopicProducerProvider topicProvider, IOptions<KafkaOptions> options) : ControllerBase
    {
        private readonly ITopicProducerProvider _topicProvider = topicProvider;
        private readonly KafkaOptions _options =  options.Value;

        [HttpPost("sendWelcomeEmail")]
        public async Task<IActionResult> TestSendingWelcomeEmail(string username, string activationLink, string email)
        {
            var producer = _topicProvider.GetProducer<UserRegistredEvent>(new Uri($"topic:{_options.UserRegistredTopic}"));
            await producer.Produce(new UserRegistredEvent(username, activationLink, email));
            return Accepted();
        }
    }
}
