namespace EmployeeService.Application.Queries.GetDoctorById
{
    public record GetDoctorResponse(string FirstName, string LastName, string Email, string PhoneNumber, TimeOnly ShiftStartTime, TimeOnly ShiftEndTime, int RoomNumber, string Specialization);
}
