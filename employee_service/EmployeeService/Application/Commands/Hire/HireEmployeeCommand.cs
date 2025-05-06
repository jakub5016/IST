namespace EmployeeService.Application.Commands.Hire
{
    public record HireEmployeeCommand(string FirstName, string LastName, string PhoneNumber, string Email, TimeOnly ShiftStartTime, TimeOnly ShiftEndTime, string Role, string Specialization="", int RoomNumber=0);
}
