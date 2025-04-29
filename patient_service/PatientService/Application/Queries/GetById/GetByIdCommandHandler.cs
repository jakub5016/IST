using Application.Shared;
using MassTransit;
using PatientService.Domain;

namespace PatientService.Application.Queries.GetById
{
    public class GetByIdCommandHandler : IConsumer<GetByIdCommand>
    {
        private readonly IPacientRepository _repository;

        public GetByIdCommandHandler(IPacientRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetByIdCommand> context)
        {
            try
            {
                var patient = await _repository.GetPatientAsync(context.Message.Id);
                if (patient == null)
                {
                    await context.RespondAsync(Result.Failure(new Error("", "")));
                    return;
                }
                await context.RespondAsync(Result<PatientResponse>.Success(new(patient.FirstName, patient.LastName, patient.PESEL, patient.PhoneNumber)));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result.Failure(new Error("Unexpected", ex.Message)));
            }
        }
    }
}
