using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain
{
    public class Patient
    {
        public Guid Id { get; set; } = new Guid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PESEL { get; set; } = string.Empty;
        public string PhoneNumber {  get; set; } = string.Empty;
        public bool IsPhoneConfirmed { get; set; } = false;
        public bool IsIdentityConfirmed { get; set; } = false;
        
        public Patient(string firstName, string lastName , string pesel, string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            PESEL = pesel;
            PhoneNumber = phoneNumber;
        }

        public Patient()
        { 
        }

        public void Update(string firstName, string lastName, string phoneNumber)
        {
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
        }
        public void ConfirmPhoneNumber () => IsPhoneConfirmed = true;

        public void ConfirmIdentity() => IsIdentityConfirmed = true;

    }
}
