CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE "Buses" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "CompanyName" character varying(100) NOT NULL,
    "TotalSeats" integer NOT NULL,
    "BusType" character varying(50) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Buses" PRIMARY KEY ("Id")
);

CREATE TABLE "Passengers" (
    "Id" uuid NOT NULL,
    "Name" character varying(100) NOT NULL,
    "MobileNumber" character varying(20) NOT NULL,
    "Email" character varying(100),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Passengers" PRIMARY KEY ("Id")
);

CREATE TABLE "Routes" (
    "Id" uuid NOT NULL,
    "FromCity" character varying(100) NOT NULL,
    "ToCity" character varying(100) NOT NULL,
    "Distance" numeric(10,2) NOT NULL,
    "EstimatedDuration" interval NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Routes" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Email" character varying(254) NOT NULL,
    "FullName" character varying(100),
    "PasswordHash" character varying(255),
    "IsEmailVerified" boolean NOT NULL DEFAULT FALSE,
    "EmailVerifiedAt" timestamp with time zone,
    "LastLoginAt" timestamp with time zone,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "Seats" (
    "Id" uuid NOT NULL,
    "BusId" uuid NOT NULL,
    "SeatNumber" character varying(10) NOT NULL,
    "Row" character varying(20) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Seats" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Seats_Buses_BusId" FOREIGN KEY ("BusId") REFERENCES "Buses" ("Id") ON DELETE CASCADE
);

CREATE TABLE "BusSchedules" (
    "Id" uuid NOT NULL,
    "BusId" uuid NOT NULL,
    "RouteId" uuid NOT NULL,
    "DepartureTime" timestamp with time zone NOT NULL,
    "ArrivalTime" timestamp with time zone NOT NULL,
    "JourneyDate" timestamp with time zone NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "Currency" character varying(3) NOT NULL DEFAULT 'USD',
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_BusSchedules" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BusSchedules_Buses_BusId" FOREIGN KEY ("BusId") REFERENCES "Buses" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BusSchedules_Routes_RouteId" FOREIGN KEY ("RouteId") REFERENCES "Routes" ("Id") ON DELETE RESTRICT
);

CREATE TABLE "OtpCodes" (
    "Id" uuid NOT NULL,
    "Email" character varying(254) NOT NULL,
    "Code" character(6) NOT NULL,
    "Purpose" character varying(20) NOT NULL,
    "IsUsed" boolean NOT NULL DEFAULT FALSE,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "AttemptCount" integer NOT NULL DEFAULT 0,
    "MaxAttempts" integer NOT NULL DEFAULT 3,
    "UserId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_OtpCodes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OtpCodes_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Tickets" (
    "Id" uuid NOT NULL,
    "SeatId" uuid NOT NULL,
    "PassengerId" uuid NOT NULL,
    "BusScheduleId" uuid NOT NULL,
    "BoardingPoint" character varying(100) NOT NULL,
    "DroppingPoint" character varying(100) NOT NULL,
    "BookingDate" timestamp with time zone NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "Currency" character varying(3) NOT NULL DEFAULT 'USD',
    "Status" text NOT NULL,
    "CancellationReason" character varying(500),
    "UserId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    CONSTRAINT "PK_Tickets" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Tickets_BusSchedules_BusScheduleId" FOREIGN KEY ("BusScheduleId") REFERENCES "BusSchedules" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Tickets_Passengers_PassengerId" FOREIGN KEY ("PassengerId") REFERENCES "Passengers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_Tickets_Seats_SeatId" FOREIGN KEY ("SeatId") REFERENCES "Seats" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Tickets_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_BusSchedules_BusId" ON "BusSchedules" ("BusId");

CREATE INDEX "IX_BusSchedules_JourneyDate" ON "BusSchedules" ("JourneyDate");

CREATE INDEX "IX_BusSchedules_RouteId_JourneyDate" ON "BusSchedules" ("RouteId", "JourneyDate");

CREATE INDEX "IX_OtpCodes_Email_Purpose_ExpiresAt" ON "OtpCodes" ("Email", "Purpose", "ExpiresAt");

CREATE INDEX "IX_OtpCodes_Email_Purpose_IsUsed" ON "OtpCodes" ("Email", "Purpose", "IsUsed");

CREATE INDEX "IX_OtpCodes_ExpiresAt" ON "OtpCodes" ("ExpiresAt");

CREATE INDEX "IX_OtpCodes_UserId" ON "OtpCodes" ("UserId");

CREATE UNIQUE INDEX "IX_Passengers_MobileNumber" ON "Passengers" ("MobileNumber");

CREATE INDEX "IX_Routes_FromCity" ON "Routes" ("FromCity");

CREATE INDEX "IX_Routes_FromCity_ToCity" ON "Routes" ("FromCity", "ToCity");

CREATE INDEX "IX_Routes_ToCity" ON "Routes" ("ToCity");

CREATE UNIQUE INDEX "IX_Seats_BusId_SeatNumber" ON "Seats" ("BusId", "SeatNumber");

CREATE INDEX "IX_Seats_BusId_Status" ON "Seats" ("BusId", "Status");

CREATE INDEX "IX_Seats_SeatNumber" ON "Seats" ("SeatNumber");

CREATE INDEX "IX_Tickets_BusScheduleId" ON "Tickets" ("BusScheduleId");

CREATE INDEX "IX_Tickets_PassengerId" ON "Tickets" ("PassengerId");

CREATE UNIQUE INDEX "IX_Tickets_SeatId" ON "Tickets" ("SeatId");

CREATE UNIQUE INDEX "IX_Tickets_SeatId_BusScheduleId_Unique" ON "Tickets" ("SeatId", "BusScheduleId") WHERE "Status" = 'Confirmed';

CREATE INDEX "IX_Tickets_UserId" ON "Tickets" ("UserId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE INDEX "IX_Users_IsActive" ON "Users" ("IsActive");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251026122755_AddAuthenticationTables', '9.0.10');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251028083536_InitialCreate', '9.0.10');

COMMIT;

