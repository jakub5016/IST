using Application.Shared;
using MassTransit;
using PatientService.Application.Command.CancelRegistration;
using PatientService.Domain;
using PatientService.Domain.Shared;
using PatientService.Infrastracture.Messaging.IntegrationEvents;

namespace PatientService.Application.Command.UndoRegistration
{
    public class CancelRegistrationCommandHandler : IConsumer<CancelPatientRegistration>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CancelRegistrationCommandHandler(IPacientRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<CancelPatientRegistration> context)
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
                _repository.DeletePatientAsync(patient);
                await _unitOfWork.SaveChangesAsync();
                await context.Publish(new PatientRegistrationCancelled(patient.Id));
            }
            catch (Exception ex) {
                await context.RespondAsync(Result.Failure(new Error("", ex.Message)));
                return;
            }


        }
    }
}
