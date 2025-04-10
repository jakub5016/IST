using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Domain
{
    public class PatientRegisteredEvent
    {
        public Guid PatientId { get; set; }
    }
}
