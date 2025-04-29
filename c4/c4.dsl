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
            
            userService = container "User Managment Service" "Odpowiada za autoryzacje użytkowników, rejestrowanie, logowanie" "Django" {
                tags "Web app"
            }

            userDB = container "UserDB" "Przechowuje informacje o użytkownikach serwisu" "Postgres"{
                tags "DB"
            }


            patientService = container "Patient Managment Service" "Odpowiada za aktualizacja danych osobowych pacjentów, ich rejestracje oraz dostarczanie danych na temat pacjentów" "ASP.NET Web API" {
                tags "Web app"
            }
           
            patientDB = container "PatientDB" "Przechowuje informacje o pacjentach kliniki" "Postgres"{
                tags "DB"
            }

           
            documentService = container "Mediacal Document Service" "Odpowiada za zarządzanie notatkami lekarzy" "ASP.NET Web API"{
                tags "Web app"

            }

            storage = container "Blob Storage" "Przechwuje notatki medyczne" "S3"{
                tags "S3"
            }

            paymentProvider = container "Payment Provider" "Jest odpowiedzialny za komunikacje z PayU API" "FastApi"{
                tags "Web app"
            }

            emailService = container "Email Service" "Jest odpowiedzialny za wysyłanie powiadomień mail" "ASP.NET Web API"{
                tags "Web app"
            }

            smsService = container "SMS Service" "Jest odpowiedzialny za wysyłanie powiadomień SMS" "FastApi"{
                tags "Web app"
            }

            appointmentService = container "Appointment Service" "Jest odpowiedzialny za zarządzanie wizytami (rezerwacja, anulowanie)" "Flask" {
                tags "Web app"
            }

            appointmentDB = container "AppointmentDB" "Przechowuje informacje o wizytach" "MongoDB"{
                tags "DB"
            }

            paymentService = container "Payment Service" "Przechowuje i zarządza danymi o płatnościach" "FastAPI"{
                tags "Web app"
            }

            paymentDB = container "PaymentDB" "Przechowuje dane o płatnościach" "Redis"{
                tags "DB"
            }

            meetingService = container "Zoom Meeting Service" "Wysyła żądania utworzenia spotkania na zoomie przez api" "Flask"{
                tags "Web app"

            }


            doctorService = container "Doctor Managment Service" "Zarządza danymi lekarzy (CRUD)" "Flask" {
                tags "Web app"
            }

            doctorDB = container "DoctorDB" "Przechowuje informacje o lekarzach" "Postgres"{
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
        
        emailService -> User "Wysyła maila" "SMTP" 
        paymentProvider -> paymentAPI "Wysyła zapytanie" "Rest API"
        meetingService -> zoomAPI "Wysyła zapytanie" "HTTP/JSON"
        smsService -> User "Wysyła sms" "przez pop3 "
        
        paymentService -> paymentDB "Aktualizuje dane na temat płatności" "tcp"
        paymentDB  -> paymentService "Pobiera dane na temat płatności" "tcp"
        patientService -> patientDB "Aktualizuje dane na temat pacjentów" "tcp"
        patientDB -> patientService "Pobiera dane na temat pacjentów" "tcp"
        doctorService -> doctorDB "Aktualizuje dane na temat lekarzy" "tcp"
        doctorDB -> doctorService "Pobiera dane na temat lekarzy" "tcp"
        appointmentService -> appointmentDB "Aktualizuje dane na temat wizyt" "tcp"
        appointmentDB -> appointmentService "Pobiera dane na temat wizyt" "tcp"
        userService -> userDB "Aktualizuje dane na temat użytkowników" "tcp" 
        userDB -> userService "Pobiera dane na temat użytkowników" "tcp"
        documentService -> storage "Wysyła zapytanie" "HTTP/JSON"

        // Event Flow
        gateway -> userService "" "przez Event Store"
        gateway -> patientService "" "przez Event Store"
        gateway -> appointmentService "" "przez Event Store"
        gateway -> documentService "" "przez Event Store"
        gateway -> doctorService "" "przez Event Store"

        userService -> gateway "" "przez Event Store" 
        patientService  -> gateway "" "przez Event Store"
        appointmentService  -> gateway "" "przez Event Store"
        documentService  -> gateway "" "przez Event Store"
       
        documentService -> emailService "" "przez Event Store"
        userService -> emailService "" "przez Event Store"
        userService -> smsService "" "przez Event Store"
        appointmentService -> meetingService "" "przez Event Store"
        meetingService -> appointmentService "" "przez Event Store"

        appointmentService -> paymentService "" "przez Event Store"
        paymentService -> appointmentService "" "przez Event Store"
       
        paymentService -> paymentProvider "" "przez Event Store"
        paymentProvider -> paymentService "" "przez Event Store"
    
        appointmentService -> emailService "Wysyła informacje o zarezerwowonej wizycie" "przez Event Store"
        appointmentService -> smsService "Wysyła żądanie o zbliżającej się wizycie" "przez Event Store"
       // kafka -> patientService
       // patientService -> kafka



        user -> clinicSystem "Używa do rezerwacji wizyt lekarskich, dostępu do dokumentacji medycznej"
        clinicStaff -> clinicSystem "Używa do zarządzania wizytami pacjentów i danymi medycznymi"
        admin -> clinicSystem "Używa do zarządzania danymi pracowników klinki"
        doctor -> clinicSystem "Używa do zamieszczania notatek lekarskich"
        //gateway -> kafka "Wysyła eventy, które są konsumowane przez mikroserwisy"
        user -> gateway "Wysyła zapytanie" "HTTP/JSON"
        clinicStaff -> gateway "Wysyła zapytanie" "HTTP/JSON"
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