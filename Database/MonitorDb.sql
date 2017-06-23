create table Procesos(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	vchTiempoEspera varchar(4) not null,
	vchNombreProceso varchar(15) not null
);
create table CorreoNotificador(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	vchCorreo VARCHAR(150) NOT NULL,
	vchPassword VARCHAR(150) NOT NULL,
	vchPuerto VARCHAR(5) NOT NULL,
	vchHost VARCHAR(50) NOT NULL
);
create table CorreosDestinatarios(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	vchCorreo varchar(150) not null,
	vchNombre varchar(200) not null,
	bitActivo BIT NOT NULL Default 1
);
create table NotificacionesEnviadas(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	id_correo INTEGER NOT NULL,
	id_proceso INTEGER NOT NULL,
	datFechaEnviado DATE NOT NULL,
	FOREIGN KEY(id_correo) REFERENCES CorreosDestinatarios(id),
	FOREIGN KEY(id_proceso) REFERENCES Procesos(id)
);