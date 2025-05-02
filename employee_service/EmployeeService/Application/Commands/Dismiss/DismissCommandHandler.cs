using EmployeeService.Application.Queries.GetById;
using EmployeeService.Core;
using EmployeeService.Domain;
using MassTransit;

namespace EmployeeService.Application.Commands.Dismiss
{
    public class DismissCommandHandler : IConsumer<DismissCommand>
    {
        private readonly IEmployeeRepository _repository;

        public DismissCommandHandler(IEmployeeRepository repository)
        {
            _repository = repository;
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
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex) {
                await context.RespondAsync(Result.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        }
    }

}
