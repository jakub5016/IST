using PatientService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Infrastracture.Database
{
    public class PatientRepository : IPacientRepository
    {
        public Task AddPatient(Patient patient)
        {
            throw new NotImplementedException();
        }

        public Task DeletePatientAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<Patient> GetPatientAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
