using Microsoft.EntityFrameworkCore;
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
        private readonly PatientContext _context;

        public PatientRepository(PatientContext context)
        {
            _context = context;
        }

        public async Task AddPatient(Patient patient)
        {
            await _context.Patients.AddAsync(patient);
        }

        public void DeletePatientAsync(Patient patient)
        {
            _context.Patients.Remove(patient);
        }

        public async Task<Patient?> GetPatientAsync(Guid id)
        {
            return await _context.Patients.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
