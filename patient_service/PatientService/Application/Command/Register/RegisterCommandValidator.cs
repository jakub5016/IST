using FluentValidation;

namespace PatientService.Application.Command.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.isAccountRegistred)
                .NotNull();

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Matches(@"^\+?[1-9]\d{1,14}$")
                .WithMessage("Phone number must be a valid E.164 international format.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Email must be valid.")
                .When(x => x.isAccountRegistred);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First Name must be not empty");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last Name must be not empty");


            RuleFor(x => x.PESEL)
                .NotEmpty()
                .Matches(@"^[0-9]{2}(0[1-9]|1[0-2]|2[0-9]|3[0-2])(0[1-9]|[12][0-9]|3[01])[0-9]{5}$")
                .WithMessage("PESEL is not valid");



        }
    }
}
