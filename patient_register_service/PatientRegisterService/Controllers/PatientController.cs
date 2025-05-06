using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;
using PatientRegisterService.Commands.Register;

namespace PatientRegisterService.Controllers
{
    [Route("")]
    [ApiController]
    public class PatientController(IScopedMediator mediator) : ControllerBase
    {
        private readonly IScopedMediator _mediator = mediator;

        [HttpPost("")]
        public async Task<IActionResult> RegisterPatientAsync(RegisterCommand request)
        {
            var client = _mediator.CreateRequestClient<RegisterCommand>();
            var response = await client.GetResponse<RegisterResponse>(request);
            return response.Message.IsSuccessful ? Ok() : BadRequest(response.Message.ValidationErrors);
        }
    }
}