namespace EmployeeService.Domain
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsFired { get; set; }
        public TimeOnly ShiftStartTime { get; set; }
        public TimeOnly ShiftEndTime { get; set; }
        public Doctor? Doctor { get; set; }
        public static Employee Hire(string firstName, string lastName, string phoneNumber, string email, TimeOnly shiftStartTime, TimeOnly shiftEndTime, Role role, string specialization = "", int room = 0)
        {
            var id = Guid.NewGuid();
            return new Employee
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                Email = email,
                ShiftStartTime = shiftStartTime,
                ShiftEndTime = shiftEndTime,
                Doctor = role == Role.Doctor ? 
                    new Doctor { Id = id, Specialization = specialization, RoomNumber = room, } 
                    : null,
            };

        }
        public void Dismiss()
        {
            IsFired = true;
        }
    }
}
