namespace EmployeeService.Domain
{
    public enum Role
    {
        Doctor, Receptionist
    }
    public static class RoleExtensions
    {
        public static Role GetRole(string role)
        {
            if(role.Equals("doctor", StringComparison.CurrentCultureIgnoreCase))
            {
                return Role.Doctor;
            }
            if(role.Equals("receptionist", StringComparison.CurrentCultureIgnoreCase))
            {
                return Role.Receptionist;
            }
            throw new ArgumentException("Invalid Role");
        }
    };
    
    
}
