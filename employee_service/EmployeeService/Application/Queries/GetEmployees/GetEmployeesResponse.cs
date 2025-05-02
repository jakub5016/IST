using EmployeeService.Application.Queries.GetById;

namespace EmployeeService.Application.Queries.GetEmployees
{
    public record GetEmployeesResponse(List<GetEmployeeResponse> Employees);
}
