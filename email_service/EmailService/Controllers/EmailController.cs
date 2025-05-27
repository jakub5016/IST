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
            var producer = _topicProvider.GetProducer<UserRegistred>(new Uri($"topic:{_options.UserRegistredTopic}"));
            await producer.Produce(new UserRegistred(username, activationLink, email));
            return Accepted();
        }
        [HttpPost("sendPasswordChangeEmail")]
        public async Task<IActionResult> TestSendingPasswordChangeEmail([FromQuery] ChangePassword changePassword)
        {
            var producer = _topicProvider.GetProducer<ChangePassword>(new Uri($"topic:{_options.ChangePasswordTopic}"));
            await producer.Produce(changePassword);
            return Accepted();
        }
        [HttpPost("sendCancelAppointmentEmail")]
        public async Task<IActionResult> TestSendingCancelAppointmentEmail([FromQuery] AppointmentCancelled request)
        {
            var producer = _topicProvider.GetProducer<AppointmentCancelled>(new Uri($"topic:{_options.AppointmentCancelledTopic}"));
            await producer.Produce(request);
            return Accepted();
        }
        [HttpPost("sendCreateAppointmentEmail")]
        public async Task<IActionResult> TestSendingCreateAppointmentEmail([FromQuery] AppointmentCreated request)
        {
            var producer = _topicProvider.GetProducer<AppointmentCreated>(new Uri($"topic:{_options.AppointmentCreatedTopic}"));
            await producer.Produce(request);
            return Accepted();
        }
        [HttpPost("sendZoomEmail")]
        public async Task<IActionResult> TestSendingZoomEmail([FromQuery] ZoomCreated request)
        {
            var producer = _topicProvider.GetProducer<ZoomCreated>(new Uri($"topic:{_options.ZoomCreatedTopic}"));
            await producer.Produce(request);
            return Accepted();
        }
    }
}
