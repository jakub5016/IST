using FluentValidation;
using PatientService.Application.Command.Register;

namespace PatientService.Application.Command.Update
{
    public class UpdateCommandValidator : AbstractValidator<UpdateCommand>
    {
        public UpdateCommandValidator()
        {
            RuleFor(x => x.PhoneNumber)
              .NotEmpty()
              .Matches(@"^\+?[1-9]\d{1,14}$")
              .WithMessage("Phone number must be a valid E.164 international format.");

            RuleFor(x => x.FirstName)
             .NotEmpty()
             .WithMessage("First Name must be not empty");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last Name must be not empty");

        }
    }
}
