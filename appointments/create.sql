CREATE TABLE patients (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
	gender VARCHAR(1),
	pesel VARCHAR(11) UNIQUE NOT NULL,
    phone VARCHAR(30) UNIQUE
);

CREATE TABLE users (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    is_verified BOOLEAN NOT NULL,
	patient_id INT NOT NULL REFERENCES patients(id) ON DELETE CASCADE
);

CREATE TABLE doctors (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    first_name VARCHAR(30) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    specialization VARCHAR(100)
);

CREATE TABLE doctor_schedules (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    weekday INT NOT NULL CHECK (weekday >= 1 AND weekday <= 7),
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
	doctor_id INT NOT NULL REFERENCES doctors(id) ON DELETE CASCADE
);

CREATE TABLE appointment_types (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    type_name VARCHAR(100) NOT NULL,
    duration_minutes INT NOT NULL,
    is_online BOOLEAN NOT NULL
);

CREATE TABLE appointments (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    start_time TIMESTAMP NOT NULL,
    end_time TIMESTAMP NOT NULL,
    status TEXT NOT NULL DEFAULT 'zaplanowana',
    created_at TIMESTAMP DEFAULT now(),
	patient_id INT NOT NULL REFERENCES patients(id) ON DELETE SET NULL,
    doctor_id INT NOT NULL REFERENCES doctors(id) ON DELETE SET NULL,
	appointment_type_id INT NOT NULL REFERENCES appointment_types(id) ON DELETE SET NULL
);
