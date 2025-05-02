using EmployeeService.Application.Queries.GetById;
using EmployeeService.Core;
using EmployeeService.Domain;
using MassTransit;

namespace EmployeeService.Application.Commands.Cancel
{
    public class CancelEmploymentCommandHandler : IConsumer<CancelEmploymentCommand>
    {
        private readonly IEmployeeRepository _repository;

        public CancelEmploymentCommandHandler(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<CancelEmploymentCommand> context)
        {
            var req = context.Message;
            try
            {
                var employee = await _repository.GetById(req.EmployeeId);
                if (employee == null)
                {
                    await context.RespondAsync(Result<GetEmployeeResponse>.Failure(Error.NotFound));
                    return;
                }
                _repository.Delete(employee);
                await _repository.SaveChangesAsync();
                await context.RespondAsync(Result.Success());
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        }
    }
}
