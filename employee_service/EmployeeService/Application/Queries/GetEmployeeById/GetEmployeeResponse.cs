namespace EmployeeService.Application.Queries.GetById
{
    public record GetEmployeeResponse(string FirstName, string LastName, string Email, string PhoneNumber, TimeOnly ShiftStartTime, TimeOnly ShiftEndTime);
    
}
