using EmployeeService.Application.Queries.GetById;
using EmployeeService.Application.Queries.GetEmployees;
using EmployeeService.Core;
using EmployeeService.Domain;
using MassTransit;

namespace EmployeeService.Application.Commands.Hire
{
    public class HireEmployeeCommandHandler(IEmployeeRepository repository, ITopicProducer<EmployeeHired> publisher) : IConsumer<HireEmployeeCommand>
    {
        private readonly IEmployeeRepository _repository = repository;
        private readonly ITopicProducer<EmployeeHired> _publisher = publisher;

        public async Task Consume(ConsumeContext<HireEmployeeCommand> context)
        {
            var req = context.Message;
            try
            {
                var role = RoleExtensions.GetRole(req.Role);
                var employee = Employee.Hire(
                    req.FirstName,
                    req.LastName,
                    req.PhoneNumber,
                    req.Email,
                    req.ShiftStartTime,
                    req.ShiftEndTime,
                    role,
                    req.Specialization,
                    req.RoomNumber);
                await _repository.Add(employee);
                await _repository.SaveChangesAsync();
                await _publisher.Produce(new EmployeeHired(employee.Id, employee.Email, req.Role.ToLower()));
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        }

    }
}
