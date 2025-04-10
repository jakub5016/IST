using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PESEL { get; set; } = string.Empty;
        public string PhoneNumber {  get; set; } = string.Empty;
        public bool IsIdentityConfirmed { get; set; }
        // ToDo - payment data
    }
}
