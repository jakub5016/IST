namespace EmployeeService.Domain.Event
{
    public record EmployeeDismissed(Guid Id, string Email, string Role, TimeOnly StartTime, TimeOnly EndTime);
}