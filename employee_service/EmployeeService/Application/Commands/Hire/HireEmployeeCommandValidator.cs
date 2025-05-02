using FluentValidation;

namespace EmployeeService.Application.Commands.Hire
{
    public class HireEmployeeCommandValidator: AbstractValidator<HireEmployeeCommand>
    {
        public HireEmployeeCommandValidator() {

            RuleFor(x => x.PhoneNumber)
               .NotEmpty()
               .Matches(@"^\+?[1-9]\d{1,14}$")
               .WithMessage("Phone number must be a valid E.164 international format.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress()
                .WithMessage("Email must be valid.");

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First Name must be not empty.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last Name must be not empty.");
        }
    }
}
