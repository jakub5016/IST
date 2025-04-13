using Application.Shared;
using MassTransit;
using PatientService.Application.Command.ConfirmNumber;
using PatientService.Domain;

namespace PatientService.Application.Command.ConfirmPatientNumber
{
    public class ConfirmNumberCommandHandler : IConsumer<ConfirmNumberCommand>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        public ConfirmNumberCommandHandler(IPacientRepository patientRepository, IUnitOfWork unitOfWork)
        {
            _repository = patientRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<ConfirmNumberCommand> context)
        {
            var req = context.Message;
            try
            {
                var patient = await _repository.GetPatientAsync(req.PatientId);
                if (patient is null)
                {
                    return;
                }
                patient.ConfirmPhoneNumber();
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
