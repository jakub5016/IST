using MassTransit;
using PatientService.Application.Command.CancelRegistration;
using PatientService.Domain;
using PatientService.Infrastracture.Messaging.IntegrationEvents;

namespace PatientService.Application.Saga
{
    public class PatientRegistrationSaga: MassTransitStateMachine<PatientRegistrationSagaData>
    {
        public State Registred { get; set; }
        public State Created { get; set; }
        public State Notified { get; set; }
        public State Failed { get; set; }
        public State Cancelled { get; set; }
 

        public Event<PatientRegistered> PatientRegistred { get; set; }
        public Event<UserCreated> UserCreated { get; set; }
        public Event<UserCreationFailed> UserCreationFailed { get; set; }
        public Event<PatientRegistrationCancelled> PatientRegistrationCancelled { get; set; }
        public Event<WelcomeEmailSent> WelcomeEmailSent { get; set; }

        public PatientRegistrationSaga()
        {
            InstanceState(x => x.CurrentState);

            Event(() => PatientRegistred, e => e.CorrelateById(m => m.Message.PatientId));
            Event(() => UserCreated, e => e.CorrelateById(m => m.Message.PatientId));
            Event(() => UserCreationFailed, e => e.CorrelateById(m => m.Message.PatientId));
            Event(() => WelcomeEmailSent, e => e.CorrelateById(m => m.Message.PatientId));
            Initially(
                When(PatientRegistred)
                    .Then(context =>
                    {
                        context.Saga.PatientId = context.Message.PatientId;
                        context.Saga.Email = context.Message.Email;
                    })
                    .IfElse(context => context.Message.IsAccountRegistred,
                        thenBinder => thenBinder
                            .Publish(context => new CreateUser(context.Message.PatientId, context.Message.Email)),
                        elseBinder => elseBinder
                            .Then(context => {
                
                            })
                    )
                    .TransitionTo(Registred)
            );


            During(Registred,
                When(UserCreated)
                   .TransitionTo(Created)
                   .Publish(context => new SendWelcomeEmail(context.Message.PatientId, context.Message.Email)),
                When(UserCreationFailed)
                    .TransitionTo(Failed)
                    .Publish(context => new CancelPatientRegistration(context.Message.PatientId)));
            
            During(Created, When(WelcomeEmailSent).TransitionTo(Notified).Finalize());
            During(Failed, When(PatientRegistrationCancelled).TransitionTo(Cancelled).Finalize());
            

        }
    }
}
