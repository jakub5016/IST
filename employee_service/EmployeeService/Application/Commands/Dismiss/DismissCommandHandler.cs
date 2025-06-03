using EmployeeService.Application.Queries.GetById;
using EmployeeService.Core;
using EmployeeService.Domain;
using EmployeeService.Domain.Event;
using MassTransit;

namespace EmployeeService.Application.Commands.Dismiss
{
    public class DismissCommandHandler : IConsumer<DismissCommand>
    {
        private readonly IEmployeeRepository _repository;
        private readonly ITopicProducer<EmployeeDismissed> _producer;

        public DismissCommandHandler(IEmployeeRepository repository, ITopicProducer<EmployeeDismissed> producer)
        {
            _repository = repository;
            _producer = producer;
        }

        public async Task Consume(ConsumeContext<DismissCommand> context)
        {
            var req = context.Message;
            try
            {
                var employee = await _repository.GetById(req.Id);
                if (employee == null)
                {
                    await context.RespondAsync(Result<GetEmployeeResponse>.Failure(Error.NotFound));
                    return;
                }
                employee.Dismiss();
                await _repository.SaveChangesAsync();
                await _producer.Produce(new EmployeeDismissed(employee.Id, employee.Email, employee.GetRole().ToString().ToLower(), employee.ShiftStartTime, employee.ShiftEndTime));
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex) {
                await context.RespondAsync(Result.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        }
    }

}
