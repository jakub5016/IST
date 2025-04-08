using EmailService.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace EmailService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController(ITopicProducerProvider topicProvider) : ControllerBase
    {
        private readonly ITopicProducerProvider _topicProvider = topicProvider;

        [HttpPost("sendWelcomeEmail")]
        public async Task<IActionResult> TestSendingWelcomeEmail(string username, string activationLink, string email)
        {
            var producer = _topicProvider.GetProducer<UserRegistredEvent>(new Uri($"topic:user_registred"));
            await producer.Produce(new UserRegistredEvent(username, activationLink, email));
            return Accepted();
        }
    }
}
