using EmployeeService.Application.Queries.GetById;
using EmployeeService.Application.Queries.GetDoctorById;
using EmployeeService.Application.Queries.GetEmployees;
using EmployeeService.Core;
using EmployeeService.Domain;
using MassTransit;

namespace EmployeeService.Application.Queries.GetDoctors
{
    public class GetDoctorsQueryHander : IConsumer<GetDoctorsQuery>
    {
        private readonly IEmployeeRepository _repository;

        public GetDoctorsQueryHander(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public async Task Consume(ConsumeContext<GetDoctorsQuery> context)
        {
            var req = context.Message;
            try
            {
                var employees = await _repository.GetAllDoctors();
                if (employees == null)
                {
                    await context.RespondAsync(Result<GetDoctorsResponse>.Failure(Error.NotFound));
                    return;
                }
                var response = new GetDoctorsResponse(
                    [.. employees
                    .Select(x => new GetDoctorResponse(
                        x.Id,
                        x.FirstName,
                        x.LastName,
                        x.Email,
                        x.PhoneNumber,
                        x.ShiftStartTime,
                        x.ShiftEndTime,
                        x.Doctor.RoomNumber,
                        x.Doctor.Specialization))
                    ]
                );
                await context.RespondAsync(Result<GetDoctorsResponse>.Success(response));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result<GetDoctorsResponse>.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        
        }
    }
}
