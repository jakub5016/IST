using EmployeeService.Application.Queries.GetById;
using EmployeeService.Core;
using EmployeeService.Domain;
using MassTransit;

namespace EmployeeService.Application.Queries.GetDoctorById
{
    public class GetDoctorByIdQueryHandler : IConsumer<GetDoctorByIdQuery>
    {
        private readonly IEmployeeRepository _repository;

        public GetDoctorByIdQueryHandler(IEmployeeRepository employeeRepository)
        {
            _repository = employeeRepository;
        }

        public async Task Consume(ConsumeContext<GetDoctorByIdQuery> context)
        {
            var req = context.Message;
            try
            {
                var employee = await _repository.GetById(req.Id);
                if (employee == null)
                {
                    await context.RespondAsync(Result<GetDoctorResponse>.Failure(Error.NotFound));
                    return;
                }
                var response = new GetDoctorResponse(
                    employee.Id,
                    employee.FirstName,
                    employee.LastName,
                    employee.Email,
                    employee.PhoneNumber,
                    employee.ShiftStartTime,
                    employee.ShiftEndTime,
                    employee.Doctor.RoomNumber,
                    employee.Doctor.Specialization
                );
                await context.RespondAsync(Result<GetDoctorResponse>.Success(response));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(Result<GetDoctorResponse>.Failure(Error.SomethingWentWrong(ex.Message)));
            }
        }
    }
}
