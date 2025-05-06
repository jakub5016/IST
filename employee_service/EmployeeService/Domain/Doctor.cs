namespace EmployeeService.Domain
{
    public class Doctor
    {
        public Guid Id { get; set; }  
        public string Specialization { get; set; }
        public int RoomNumber { get; set; }
        public Employee? Employee { get; set; }
    }
}
