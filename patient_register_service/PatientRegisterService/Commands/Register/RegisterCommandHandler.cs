using FluentValidation;
using MassTransit;
using PatientRegisterService.Events;

namespace PatientRegisterService.Commands.Register
{
    public class RegisterCommandHandler : IConsumer<RegisterCommand>
    {
        private readonly ITopicProducer<PatientRegister> _publisher;
        private readonly IValidator<RegisterCommand> _registerValidator;

        public RegisterCommandHandler(ITopicProducer<PatientRegister> publisher, IValidator<RegisterCommand> registerValidator)
        {
            _publisher = publisher;
            _registerValidator = registerValidator;
        }

        public async Task Consume(ConsumeContext<RegisterCommand> context)
        {
            var req = context.Message;
            var validationResults = await _registerValidator.ValidateAsync(req);
            if (!validationResults.IsValid)
            {
                List<ValidationError> errors = [..validationResults.Errors
                    .Select(x => new ValidationError(x.PropertyName, x.ErrorMessage))
                ];
                await context.RespondAsync(RegisterResponse.Failure(errors));
                return;
            }
            await _publisher.Produce(new PatientRegister
                (
                    req.FirstName,
                    req.LastName,
                    req.PESEL,
                    req.PhoneNumber,
                    req.Email,
                    req.isAccountRegistred
                ));
            var response = new RegisterResponse(true, []);
            await context.RespondAsync(RegisterResponse.Success());
        }
    }
}
