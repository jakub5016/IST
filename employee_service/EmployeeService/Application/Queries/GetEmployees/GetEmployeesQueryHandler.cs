using EmployeeService.Application.Queries.GetById;
using EmployeeService.Core;
using EmployeeService.Domain;
using MassTransit;

namespace EmployeeService.Application.Queries.GetEmployees
{
    public class GetEmployeesQueryHandler : IConsumer<GetEmployeesQuery>
    {
        private readonly IEmployeeRepository _repository;

        public GetEmployeesQueryHandler(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetEmployeesQuery> context)
        {
            var req = context.Message;
            try
            {
                var employees = await _repository.GetAll();
                if (employees == null)
                {
                    await context.RespondAsync(Result<GetEmployeesResponse>.Failure(Error.NotFound));
                    return;
                }
                var response = new GetEmployeesResponse(
                    [.. employees
                    .Select(x => new GetEmployeeResponse(
                        x.Id,
                        x.FirstName,
                        x.LastName,
                        x.Email,
                        x.PhoneNumber,
                        x.ShiftStartTime,
                        x.ShiftEndTime))
                    ]
                );
                await context.RespondAsync(Result<GetEmployeesResponse>.Success(response));
            }
            catch (Exception ex) {
                await context.RespondAsync(Result<GetEmployeesResponse>.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        }
    }
}
