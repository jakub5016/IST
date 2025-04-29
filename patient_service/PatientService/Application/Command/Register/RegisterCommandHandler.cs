using Application.Shared;
using MassTransit;
using PatientService.Domain;
using PatientService.Domain.Shared;

namespace PatientService.Application.Command.Register
{
    public class RegisterCommandHandler : IConsumer<RegisterCommand>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITopicProducer<PatientRegistered> _publisher;

        public RegisterCommandHandler(IPacientRepository repository, IUnitOfWork unitOfWork, ITopicProducer<PatientRegistered> publisher)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _publisher = publisher;
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
                await _publisher.Produce(new PatientRegistered(patient.Id, req.Email,req.isAccountRegistred));
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex) { 
                await context.RespondAsync(Result.Failure(new Error("",$"{ex.Message}")));
            }
        }
    }
}
