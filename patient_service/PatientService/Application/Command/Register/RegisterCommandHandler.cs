using Application.Shared;
using MassTransit;
using PatientService.Domain;

namespace PatientService.Application.Command.Register
{
    public class RegisterCommandHandler : IConsumer<RegisterCommand>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        public RegisterCommandHandler(IPacientRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<RegisterCommand> context)
        {
            try
            {
                var req = context.Message;
                var patient = new Patient(req.FirstName,
                                          req.LastName,
                                          req.PESEL,
                                          req.PhoneNumber);
                await _repository.AddPatient(patient);
                await _unitOfWork.SaveChangesAsync();
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex) { 
                await context.RespondAsync(Result.Failure(new Error("",$"{ex.Message}")));
            }
        }
    }
}
