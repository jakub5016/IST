using FluentValidation;
using MassTransit;

namespace PatientService.Application.Utils
{
    public class ValidationFilter<T> : IFilter<ConsumeContext<T>> where T : class
    {
        private readonly IValidator<T> _validator;

        public ValidationFilter(IValidator<T> validator)
        {
            _validator = validator;
        }
        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("ValidationFilter");
        }

        public async Task Send(ConsumeContext<T> context, IPipe<ConsumeContext<T>> next)
        {
            var validationResult = await _validator.ValidateAsync(context.Message);

            if (!validationResult.IsValid)
            {
                await context.RespondAsync(validationResult.Errors.Select(x=>x.ErrorMessage));
                throw new ValidationException(validationResult.Errors);
            }

            await next.Send(context);
        }
    }
}
