using Application.Shared;
using MassTransit;
using PatientService.Domain;
using PatientService.Domain.Shared;
using PatientService.Infrastracture.Messaging.IntegrationEvents;

namespace PatientService.Application.Command.ConfirmPatientIdentity
{
    public class ConfirmIdentityCommandHandler : IConsumer<ConfirmIdentityCommand>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITopicProducer<IdentityConfirmed> _publisher;

        public ConfirmIdentityCommandHandler(IPacientRepository repository, IUnitOfWork unitOfWork, ITopicProducer<IdentityConfirmed> publisher)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
        }

        public async Task Consume(ConsumeContext<ConfirmIdentityCommand> context)
        {
            var req = context.Message;
            try
            {
                var patient = await _repository.GetPatientAsync(req.PatientId);
                if (patient is null)
                {
                    await context.RespondAsync(Result.Failure(new Error("404", "Not found")));
                    return;
                }
                patient.ConfirmIdentity();
                await _unitOfWork.SaveChangesAsync();
                await _publisher.Produce(new IdentityConfirmed(req.PatientId));
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result.Failure(new Error("Unexpected", ex.Message)));
            }
        }
    }
}
