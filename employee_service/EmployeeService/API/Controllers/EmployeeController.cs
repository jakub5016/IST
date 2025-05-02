using EmployeeService.Application.Commands.Cancel;
using EmployeeService.Application.Commands.Dismiss;
using EmployeeService.Application.Commands.Hire;
using EmployeeService.Application.Queries.GetById;
using EmployeeService.Application.Queries.GetDoctorById;
using EmployeeService.Application.Queries.GetDoctors;
using EmployeeService.Application.Queries.GetEmployees;
using EmployeeService.Core;
using FluentValidation;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeService.API.Controllers
{
    [Route("employee")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IValidator<HireEmployeeCommand> _validator;
        private readonly IScopedMediator _mediator;
        private readonly ITopicProducer<CancelEmploymentCommand> _producer;


        public EmployeeController(IValidator<HireEmployeeCommand> validator, IScopedMediator mediator, ITopicProducer<CancelEmploymentCommand> producer)
        {
            _validator = validator;
            _mediator = mediator;
            _producer = producer;
        }

        [HttpPost("hire")]
        public async Task<IActionResult> HireEmployeeAsync(HireEmployeeCommand request)
        {
            var validationResults = await _validator.ValidateAsync(request);
            if (validationResults.IsValid)
            {
                var client = _mediator.CreateRequestClient<HireEmployeeCommand>();
                var response = await client.GetResponse<Result>(request);
                return response.Message.IsSuccess ? Ok() : BadRequest(response.Message.Error.Description);
            }
            var errors = validationResults.Errors
                .Select(x => new { propertyName = x.PropertyName, errorMessage = x.ErrorMessage })
                .ToList();
            return BadRequest(errors);
        }
        
        [HttpDelete("dismiss/{id}")]
        public async Task<IActionResult> DismissEmployee(Guid id) {
            var client = _mediator.CreateRequestClient<DismissCommand>();
            var response = await client.GetResponse<Result>(new DismissCommand(id));
            return GetResponse(response);
        }
        [HttpDelete("{id}")]
        public IActionResult CancelEmployee(Guid id)
        {
            _producer.Produce(new CancelEmploymentCommand(id));
            return Accepted();
        }
        [HttpGet("doctor/all")]
        public async Task<IActionResult> GetDoctors() {
            var client = _mediator.CreateRequestClient<GetDoctorsQuery>();
            var response = await client.GetResponse<Result<GetDoctorsResponse>>(new GetDoctorsQuery());
            return GetResponse(response);
        }
        [HttpGet("doctor/{id}")]
        public async Task<IActionResult> GetDoctors(Guid id)
        {
            var client = _mediator.CreateRequestClient<GetDoctorByIdQuery>();
            var response = await client.GetResponse<Result<GetDoctorResponse>>(new GetDoctorByIdQuery(id));
            return GetResponse(response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetEmployeesAsync() {
            var client = _mediator.CreateRequestClient<GetEmployeesQuery>();
            var response = await client.GetResponse<Result<GetEmployeesResponse>>(new GetEmployeesQuery());
            return GetResponse(response);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeByIdAsync(Guid id)
        {
            var client = _mediator.CreateRequestClient<GetByIdQuery>();
            var response = await client.GetResponse<Result<GetEmployeeResponse>>(new GetByIdQuery(id));
            return GetResponse(response);
        }
        private IActionResult GetResponse(Response<Result> response)
        {
            return response.Message.IsSuccess ? Ok() : response.Message.Error.Code == "404" ? NotFound() : BadRequest();
        }
        private IActionResult GetResponse<T>(Response<Result<T>> response) where T : class
        {
            return response.Message.IsSuccess ? Ok(response.Message.Value) : response.Message.Error.Code == "404" ? NotFound() : BadRequest();
        }
    }
}
