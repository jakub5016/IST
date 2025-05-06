using EmployeeService.Application.Queries.GetDoctorById;

namespace EmployeeService.Application.Queries.GetDoctors
{
    public record GetDoctorsResponse(List<GetDoctorResponse> Doctors);
}
