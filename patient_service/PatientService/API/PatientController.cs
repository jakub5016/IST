using Application.Shared;
using FluentValidation;
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
        private readonly IValidator<RegisterCommand> _registerValidator;
        private readonly IValidator<UpdateCommand> _updateValidator;


        public PatientController(IScopedMediator mediator, IValidator<RegisterCommand> registerValidator, IValidator<UpdateCommand> updateValidator)
        {
            _mediator = mediator;
            _registerValidator = registerValidator;
            _updateValidator = updateValidator;
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPatientAsync(RegisterCommand request) 
        {
            var validationResults = await _registerValidator.ValidateAsync(request);
            if (validationResults.IsValid) {
                await _mediator.Send(request);
                return Ok();
            }
            var errors = validationResults.Errors.Select(x => new { propertyName = x.PropertyName, errorMessage = x.ErrorMessage }).ToList();
            return BadRequest(errors);


        }

        [HttpPut]
        public async Task<IActionResult> UpdatePatientDataAsync(UpdateCommand request)
        {
            var validationResults = await _updateValidator
                .ValidateAsync(request);
            if (validationResults.IsValid)
            {
                await _mediator.Send(request);
                return Accepted();
            }
            var errors = validationResults.Errors.Select(x => new { propertyName = x.PropertyName, errorMessage = x.ErrorMessage }).ToList();
            return BadRequest(errors);
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
