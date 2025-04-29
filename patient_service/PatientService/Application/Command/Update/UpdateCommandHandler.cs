using Application.Shared;
using MassTransit;
using PatientService.Domain;
using PatientService.Domain.Shared;

namespace PatientService.Application.Command.Update
{
    public class UpdateCommandHandler : IConsumer<UpdateCommand>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCommandHandler(IPacientRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<UpdateCommand> context)
        {
            var req = context.Message;
            try
            {
                var patient = await _repository.GetPatientAsync(req.Id);
                if (patient is null)
                {
                    await context.RespondAsync(Result.Failure(new Error("404", "Not found")));
                    return;
                }
                patient.Update(req.FirstName, req.LastName, req.PhoneNumber);
                await _unitOfWork.SaveChangesAsync();
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex) {
                await context.RespondAsync(Result.Failure(new Error("Unexpected", ex.Message)));
            }
        }
    }
}
