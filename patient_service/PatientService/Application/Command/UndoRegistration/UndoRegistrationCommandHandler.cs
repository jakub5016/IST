using Application.Shared;
using MassTransit;
using PatientService.Domain;
using PatientService.Infrastracture.Messaging.IntegrationEvents;

namespace PatientService.Application.Command.UndoRegistration
{
    public class UndoRegistrationCommandHandler : IConsumer<UserRegisterCanceledEvent>
    {
        private readonly IPacientRepository _repository;
        private readonly IUnitOfWork _unitOfWork;

        public UndoRegistrationCommandHandler(IPacientRepository repository, IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task Consume(ConsumeContext<UserRegisterCanceledEvent> context)
        { //To Do
            var req = context.Message;
            try
            {
                var patient = await _repository.GetPatientAsync(req.PatientId);
                if (patient is null)
                {
                    return;
                }
                _repository.DeletePatientAsync(patient);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex) {

            }


        }
    }
}
