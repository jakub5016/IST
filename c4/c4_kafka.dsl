workspace {
    model {
        user = person "Użytkownik" "Pacjent korzystający z systemu"
        doctor = person "Lekarz" "Lekarz korzystający z systemu"

        clinicStaff = person "Personel Kliniki" "Personel medyczny i administracyjny pracujący w klinikce"
        admin = person "Administrator Systemu" "Administrator zarządzający pracownikami"

        clinicSystem = softwareSystem "System Zarządzania Kliniką" "System do zarządzania klinką, wizytami lekarskimi i danymi pacjentów" {
            tags "System Kliniki"
            
            gateway = container "Backend for Frontend" "Daje dostęp aktorom do wszystkich funkcjonalności systemu przez Rest API. Przekierowuje żądania do odpowiednich mikreoserwisów." "Flask" {                
                tags "Web app"
            }
            
            kafka = container "Event Store" "Przechowuje eventy, których pojawienie się triggeruje dane akcje na konkretnych mikroserwisach" "Apache Kafka" {
                tags "Queue"
            }
            
            userService = container "User Managment Service" "" "Django" {
                tags "Web app"
            }

            userDB = container "UserDB" "" "Postgres"{
                tags "DB"
            }


            patientService = container "Patient Managment Service" "" "ASP.NET Web API" {
                tags "Web app"
            }
           
            patientDB = container "PatientDB" "" "Postgres"{
                tags "DB"
            }

           
            documentService = container "Mediacal Document Service" "" "ASP.NET Web API"{
                tags "Web app"

            }

            storage = container "Blob Storage" "" "S3"{
                tags "S3"
            }

            paymentProvider = container "Payment Provider" "" "FastApi"{
                tags "Web app"
            }

            emailService = container "Email Service" " " "ASP.NET Web API"{
                tags "Web app"
            }

            smsService = container "SMS Service" "" "FastApi"{
                tags "Web app"
            }

            appointmentService = container "Appointment Service" "" "Flask" {
                tags "Web app"
            }

            appointmentDB = container "AppointmentDB" "" "MongoDB"{
                tags "DB"
            }

            paymentService = container "Payment Service" "" "FastAPI"{
                tags "Web app"
            }

            paymentDB = container "PaymentDB" "" "Redis"{
                tags "DB"
            }

            meetingService = container "Zoom Meeting Service" "" "Flask"{
                tags "Web app"

            }


            doctorService = container "Doctor Managment Service" "" "Flask" {
                tags "Web app"
            }

            doctorDB = container "DoctorDB" "" "Postgres"{
                tags "DB"
            }
                                paymentAPI = container "PayU API" "Zewnętrzny serwis płatności (PayU)" {
            tags "System Zewnętrzny"
        
                    }
        zoomAPI = container "Zoom API" "Usługa wideokonferencji do konsultacji online" {
            tags "System Zewnętrzny"
        }

       

        }

       
        // Storage Flow
        
        emailService -> User ""
        paymentProvider -> paymentAPI "" "Rest API"
        meetingService -> zoomAPI "" "przez Rest API"
        smsService -> User "" "przez "
        
        paymentService -> paymentDB ""
        paymentDB  -> paymentService ""
        patientService -> patientDB ""
        patientDB -> patientService ""
        doctorService -> doctorDB ""
        doctorDB -> doctorService ""
        appointmentService -> appointmentDB ""
        appointmentDB -> appointmentService ""
        userService -> userDB "" 
        userDB -> userService ""
        documentService -> storage "" "przez Rest API"

        // Event Flow
        gateway -> kafka ""
        kafka -> gateway ""

        documentService -> kafka ""
        kafka -> documentService ""
        userService -> kafka ""
        kafka -> userService ""
        kafka -> appointmentService ""
        appointmentService -> kafka
        patientService -> kafka
        kafka -> patientService
        kafka -> paymentService ""
        paymentService -> kafka ""
        doctorService -> kafka ""
        kafka -> doctorService ""
        emailService -> kafka ""
        kafka -> emailService ""
        smsService -> kafka ""
        kafka -> smsService ""
        meetingService -> kafka ""
        kafka -> meetingService ""
       // kafka -> patientService
       // patientService -> kafka
        paymentProvider -> kafka 
        kafka -> paymentProvider
        


        user -> clinicSystem "Używa do rezerwacji wizyt lekarskich, dostępu do dokumentacji medycznej"
        clinicStaff -> clinicSystem "Używa do zarządzania wizytami pacjentów i danymi medycznymi"
        admin -> clinicSystem "Używa do zarządzania danymi pracowników klinki"
        doctor -> clinicSystem "Używa do zamieszczania notatek lekarskich"
        //gateway -> kafka "Wysyła eventy, które są konsumowane przez mikroserwisy"
        user -> gateway "Używa do rezerwacji wizyt lekarskich, dostępu do dokumentacji medycznej"
        clinicStaff -> gateway "Używa do zarządzania wizytami pacjentów i danymi medycznymi"
        admin -> gateway "Używa do zarządzania danymi pracowników klinki"
        doctor -> gateway "Używa do zamieszczania notatek lekarskich"
    }

    views {
        systemContext clinicSystem "ClinicSystemContext" {
            include *
            autoLayout
        }
        container clinicSystem "ClinicSystemContainers" {
            include *
            autoLayout
        }
    
        styles {
            element "Web app"{
                background #7DC032

            }
            element "Person" {
                shape Person
                background #08427B
                color #ffffff
            }
            element "DB"{
                shape Cylinder
                background #64932E

            }
            element "Queue"{
                shape Pipe
            }
            element "S3" {
                shape Folder
                background #64932E
            }
            element "System Kliniki" {
                background #1168BD
                color #ffffff
            }
            element "System Zewnętrzny" {
                background #999999
                color #ffffff
            }
        }
    }
}