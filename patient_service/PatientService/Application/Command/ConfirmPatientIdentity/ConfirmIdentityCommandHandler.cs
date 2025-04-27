using Application.Shared;
using MassTransit;
using PatientService.Domain;
using PatientService.Domain.Shared;

namespace PatientService.Application.Command.ConfirmPatientIdentity
{
    public class ConfirmIdentityCommandHandler : IConsumer<ConfirmIdentityCommand>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public ConfirmIdentityCommandHandler(IPacientRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
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
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result.Failure(new Error("", ex.Message)));
            }
        }
    }
}
