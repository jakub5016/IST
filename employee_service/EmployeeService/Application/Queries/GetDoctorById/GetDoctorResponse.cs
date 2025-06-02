namespace EmployeeService.Application.Queries.GetDoctorById
{
    public record GetDoctorResponse(Guid DoctorId, string FirstName, string LastName, string Email, string PhoneNumber, TimeOnly ShiftStartTime, TimeOnly ShiftEndTime, int RoomNumber, string Specialization);
}
