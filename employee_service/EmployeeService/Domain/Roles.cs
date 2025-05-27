namespace EmployeeService.Domain
{
    public enum Role
    {
        Doctor, Employee
    }
    public static class RoleExtensions
    {
        public static Role GetRole(string role)
        {
            if(role.Equals("doctor", StringComparison.CurrentCultureIgnoreCase))
            {
                return Role.Doctor;
            }
            if(role.Equals("employee", StringComparison.CurrentCultureIgnoreCase))
            {
                return Role.Employee;
            }
            throw new ArgumentException("Invalid Role");
        }
    };
    
    
}
