using DocumentService.Application.Commands.UploadDocument;
using DocumentService.Application.Queries.GetDocument;
using DocumentService.Application.Queries.GetDocuments;
using DocumentService.Application.Utils;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DocumentService.Controllers
{
    [Route("")]
    [ApiController]
    public class DocumentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DocumentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("documents/{appointmentId}")]
        public async Task<IActionResult> GetDocuments(Guid appointmentId) {
            var client = _mediator.CreateRequestClient<GetDocumentsQuery>();
            var response = await client.GetResponse<Result<GetDocumentsResponse>>(new GetDocumentsQuery(appointmentId));
            return response.Message.IsSuccess ? Ok(response.Message.Value) : BadRequest(response.Message.Error.Description);
        }
        [HttpGet("document/{id}")]
        public async Task<IActionResult> GetDocumentAsync(Guid id)
        {
            var client = _mediator.CreateRequestClient < GetDocumentQuery>();
            var response = await client.GetResponse<Result<GetDocumentUrlResponse>>(new GetDocumentQuery(id));
            return response.Message.IsSuccess ? Ok(response.Message.Value) : BadRequest(response.Message.Error.Description);
        }
        [HttpPost("document")]
        public async Task<IActionResult> UploadDocumentAsync([FromForm] IFormFileCollection files, Guid appointmentId, string patientEmail, string name)
        {

            await _mediator.Send(new UploadDocumentCommand(files[0].OpenReadStream(),appointmentId,patientEmail,name));
            return Accepted();
        }
    }
}
