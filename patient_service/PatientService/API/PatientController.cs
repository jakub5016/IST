﻿using Application.Shared;
using FluentValidation;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;
using PatientService.API.Auth;
using PatientService.Application.Command.ConfirmNumber;
using PatientService.Application.Command.ConfirmPatientIdentity;
using PatientService.Application.Command.Register;
using PatientService.Application.Command.Update;
using PatientService.Application.Queries.GetById;
using PatientService.Infrastracture.Messaging.IntegrationEvents;

namespace PatientService.API
{
    [Route("")]
    [ApiController]

    public class PatientController : ControllerBase
    {
        private readonly IScopedMediator _mediator;
        private readonly IValidator<RegisterCommand> _registerValidator;
        private readonly IValidator<UpdateCommand> _updateValidator;
        private readonly ITopicProducer<UserCreationFailed> _producer;

        public PatientController(IScopedMediator mediator, IValidator<RegisterCommand> registerValidator, IValidator<UpdateCommand> updateValidator, ITopicProducer<UserCreationFailed> producer)
        {
            _mediator = mediator;
            _registerValidator = registerValidator;
            _updateValidator = updateValidator;
            _producer = producer;
        }
        [RoleCheck(Roles.Employee)]
        [HttpPost]
        public async Task<IActionResult> RegisterPatientAsync(RegisterCommand request)
        {
            var validationResults = await _registerValidator.ValidateAsync(request);
            if (validationResults.IsValid)
            {
                var client = _mediator.CreateRequestClient<RegisterCommand>();
                var response = await client.GetResponse<Result>(request);
                return response.Message.IsSuccess ? Ok() : BadRequest(response.Message.Error.Description);
            }
            var errors = validationResults.Errors
                .Select(x => new { propertyName = x.PropertyName, errorMessage = x.ErrorMessage })
                .ToList();
            return BadRequest(errors);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> TestCancelRegistration(Guid id)
        {
            await _producer.Produce(new UserCreationFailed(id));
            return Accepted();
        }
        [RoleCheck(Roles.Employee)]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientDataAsync(Guid id, string firstName, string lastName, string phoneNumber)
        {
            var request = new UpdateCommand(id, firstName, lastName, phoneNumber);
            var validationResults = await _updateValidator
                .ValidateAsync(request);
            if (validationResults.IsValid)
            {
                var client = _mediator.CreateRequestClient<UpdateCommand>();
                var response = await client.GetResponse<Result>(request);
                return response.Message.IsSuccess ? Ok() : NoContent();
            }
            var errors = validationResults.Errors
                .Select(x => new { propertyName = x.PropertyName, errorMessage = x.ErrorMessage })
                .ToList();
            return BadRequest(errors);
        }

        [RoleCheck(Roles.Patient, Roles.Doctor, Roles.Employee)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientAsync(Guid id)
        {
            var client = _mediator.CreateRequestClient<GetByIdCommand>();
            var response = await client.GetResponse<Result<PatientResponse>>(new(id));
            return response.Message.IsSuccess ? Ok(response.Message.Value) : NoContent();
        }
        //[RoleCheck(Roles.Employee)]
        [HttpPut("{id}/confirm-identity")]
        public async Task<IActionResult> ConfirmIdentityAsync(Guid id)
        {
            var client = _mediator.CreateRequestClient<ConfirmIdentityCommand>();
            var response = await client.GetResponse<Result>(new(id));
            return GetResponse(response);
        }
        [RoleCheck(Roles.Patient)]
        [HttpPut("{id}/confirm-phone")]
        public async Task<IActionResult> ConfirmPhoneNumberAsync(Guid id)
        {
            var client = _mediator.CreateRequestClient<ConfirmNumberCommand>();
            var response = await client.GetResponse<Result>(new(id));
            return GetResponse(response);
        }

        private IActionResult GetResponse(Response<Result> response)
        {
            return response.Message.IsSuccess ? Ok() : response.Message.Error.Code == "404" ? NotFound() : BadRequest();
        }
    }
}
