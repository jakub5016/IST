using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PatientService.Infrastracture.Messaging.IntegrationEvents
{
    public record UserRegisterCanceledEvent(Guid PatientId);
}
