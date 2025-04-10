using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain
{
    public interface IPacientRepository
    {
        Task<Patient?> GetPatientAsync(Guid id);
        Task AddPatient(Patient patient);
        void DeletePatientAsync(Patient patient);
    }
}
