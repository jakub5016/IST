using EmployeeService.Application.Queries.GetEmployees;
using EmployeeService.Core;
using EmployeeService.Domain;
using MassTransit;

namespace EmployeeService.Application.Queries.GetById
{
    public class GetByIdQueryHandler(IEmployeeRepository repository) : IConsumer<GetByIdQuery>
    {
        private readonly IEmployeeRepository _repository = repository;

        public async Task Consume(ConsumeContext<GetByIdQuery> context)
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
                var response = new GetEmployeeResponse(
                    employee.Id,
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.PhoneNumber,
                    employee.ShiftStartTime,
                    employee.ShiftEndTime
                );       
                await context.RespondAsync(Result<GetEmployeeResponse>.Success(response));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result<GetEmployeeResponse>.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        }
    }
    
}
