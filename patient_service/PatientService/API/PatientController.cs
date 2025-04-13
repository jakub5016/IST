using Application.Shared;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;
using PatientService.Application.Command.ConfirmNumber;
using PatientService.Application.Command.ConfirmPatientIdentity;
using PatientService.Application.Command.Register;
using PatientService.Application.Command.Update;
using PatientService.Application.Queries.GetById;

namespace PatientService.API.API
{
    [Route("api/patients")]
    [ApiController]

    public class PatientController : ControllerBase
    {
        private readonly IScopedMediator _mediator;

        public PatientController(IScopedMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPatientAsync(RegisterCommand request) 
        {
            await _mediator.Send(request);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePatientDataAsync(UpdateCommand request)
        {
            await _mediator.Send(request);
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientAsync(Guid id) 
        {
            var client = _mediator.CreateRequestClient<GetByIdCommand>();
            var response = await client.GetResponse<Result<PatientResponse>>(new (id));
            return response.Message.IsSuccess ? Ok(response.Message.Value): NoContent();
        }

        [HttpPut("{id}/confirm-identity")]
        public async Task<IActionResult> ConfirmIdentityAsync(Guid id)
        {
            await _mediator.Send(new ConfirmIdentityCommand(id));
            return Ok();
        }

        [HttpPut("{id}/confirm-phone")]
        public async Task<IActionResult> ConfirmPhoneNumberAsync(Guid id)
        {
            await _mediator.Send(new ConfirmNumberCommand(id));
            return Ok();
        }

    }
}
