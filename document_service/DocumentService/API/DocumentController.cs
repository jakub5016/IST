using DocumentService.API.Auth;
using DocumentService.Application.Appointments.Commands.Finish;
using DocumentService.Application.Documents.Commands.UploadDocument;
using DocumentService.Application.Documents.Queries.GetDocument;
using DocumentService.Application.Documents.Queries.GetDocuments;
using DocumentService.Application.Utils;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Mvc;

namespace DocumentService.API
{
    [Route("")]
    [ApiController]
    public class DocumentController(IMediator mediator, ITopicProducer<FinishAppointmentCommand> producer) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly ITopicProducer<FinishAppointmentCommand> _producer = producer;

        [RoleCheck(Roles.Doctor, Roles.Patient)]
        [HttpGet("documents/{appointmentId}")]
        public async Task<IActionResult> GetDocuments(Guid appointmentId)
        {
            var client = _mediator.CreateRequestClient<GetDocumentsQuery>();
            var response = await client.GetResponse<Result<GetDocumentsResponse>>(new GetDocumentsQuery(appointmentId));
            return response.Message.IsSuccess ? Ok(response.Message.Value) : BadRequest(response.Message.Error.Description);
        }

        [RoleCheck(Roles.Doctor, Roles.Patient)]
        [HttpGet("document/{id}")]
        public async Task<IActionResult> GetDocumentAsync(Guid id)
        {
            var client = _mediator.CreateRequestClient<GetDocumentQuery>();
            var response = await client.GetResponse<Result<GetDocumentUrlResponse>>(new GetDocumentQuery(id));
            return response.Message.IsSuccess ? Ok(response.Message.Value) : BadRequest(response.Message.Error.Description);
        }

       // [RoleCheck(Roles.Doctor)]
        [HttpPost("document")]
        public async Task<IActionResult> UploadDocumentAsync([FromForm] IFormFileCollection files, Guid appointmentId, string name)
        {
            var client = _mediator.CreateRequestClient<UploadDocumentCommand>();
            var response = await client.GetResponse<Result>(new UploadDocumentCommand(files[0].OpenReadStream(), appointmentId, name));
            return response.Message.IsSuccess ? Ok(response.Message) : BadRequest(response.Message.Error.Description);
        }
        [HttpPost("appointment_test")]
        public async Task<IActionResult> FinishAppointment(FinishAppointmentCommand request)
        {
            await _producer.Produce(request);
            return Accepted();
        }

    }
}
