namespace EmployeeService.Domain.Event
{
    public record EmployeeHired(Guid EmployeeId, string Email, string Role, TimeOnly StartTime, TimeOnly EndTime);
}
