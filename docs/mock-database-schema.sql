-- Future SQL schema draft for the Storage Company showcase project.
-- The running API uses in-memory repositories, but these tables show how a real database could look later.

CREATE TABLE Customers (
    CustomerId UNIQUEIDENTIFIER PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(50) NULL,
    PasswordHash NVARCHAR(500) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    IsActive BIT NOT NULL
);

CREATE TABLE Facilities (
    FacilityId UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    Address NVARCHAR(255) NOT NULL,
    City NVARCHAR(100) NOT NULL,
    PostalCode NVARCHAR(30) NOT NULL,
    Country NVARCHAR(100) NOT NULL,
    PhoneNumber NVARCHAR(50) NULL,
    Email NVARCHAR(255) NULL,
    AccessHours NVARCHAR(100) NULL,
    OfficeHours NVARCHAR(100) NULL,
    HasParking BIT NOT NULL,
    HasElevator BIT NOT NULL,
    HasCCTV BIT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL
);

CREATE TABLE StorageUnitTypes (
    UnitTypeId UNIQUEIDENTIFIER PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    SizeInSquareMeters DECIMAL(8,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    RecommendedFor NVARCHAR(500) NULL,
    CreatedAtUtc DATETIME2 NOT NULL
);

CREATE TABLE StorageUnits (
    StorageUnitId UNIQUEIDENTIFIER PRIMARY KEY,
    FacilityId UNIQUEIDENTIFIER NOT NULL,
    UnitTypeId UNIQUEIDENTIFIER NOT NULL,
    UnitNumber NVARCHAR(50) NOT NULL,
    Floor INT NOT NULL,
    MonthlyPrice DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    IsClimateControlled BIT NOT NULL,
    IsDriveUp BIT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_StorageUnits_Facilities FOREIGN KEY (FacilityId) REFERENCES Facilities(FacilityId),
    CONSTRAINT FK_StorageUnits_StorageUnitTypes FOREIGN KEY (UnitTypeId) REFERENCES StorageUnitTypes(UnitTypeId)
);

CREATE TABLE Reservations (
    ReservationId UNIQUEIDENTIFIER PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    StorageUnitId UNIQUEIDENTIFIER NOT NULL,
    ReservationDateUtc DATETIME2 NOT NULL,
    MoveInDateUtc DATETIME2 NOT NULL,
    ExpiresAtUtc DATETIME2 NULL,
    Status NVARCHAR(30) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_Reservations_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Reservations_StorageUnits FOREIGN KEY (StorageUnitId) REFERENCES StorageUnits(StorageUnitId)
);

CREATE TABLE Rentals (
    RentalId UNIQUEIDENTIFIER PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    StorageUnitId UNIQUEIDENTIFIER NOT NULL,
    StartDateUtc DATETIME2 NOT NULL,
    EndDateUtc DATETIME2 NULL,
    MonthlyPrice DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_Rentals_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Rentals_StorageUnits FOREIGN KEY (StorageUnitId) REFERENCES StorageUnits(StorageUnitId)
);

CREATE TABLE Invoices (
    InvoiceId UNIQUEIDENTIFIER PRIMARY KEY,
    RentalId UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    InvoiceNumber NVARCHAR(100) NOT NULL UNIQUE,
    Amount DECIMAL(10,2) NOT NULL,
    DueDateUtc DATETIME2 NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_Invoices_Rentals FOREIGN KEY (RentalId) REFERENCES Rentals(RentalId),
    CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId)
);

CREATE TABLE Payments (
    PaymentId UNIQUEIDENTIFIER PRIMARY KEY,
    RentalId UNIQUEIDENTIFIER NOT NULL,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    InvoiceId UNIQUEIDENTIFIER NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDateUtc DATETIME2 NOT NULL,
    PaymentMethod NVARCHAR(30) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    TransactionReference NVARCHAR(100) NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_Payments_Rentals FOREIGN KEY (RentalId) REFERENCES Rentals(RentalId),
    CONSTRAINT FK_Payments_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_Payments_Invoices FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId)
);

CREATE TABLE AccessCodes (
    AccessCodeId UNIQUEIDENTIFIER PRIMARY KEY,
    RentalId UNIQUEIDENTIFIER NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    IsActive BIT NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    ExpiresAtUtc DATETIME2 NULL,
    CONSTRAINT FK_AccessCodes_Rentals FOREIGN KEY (RentalId) REFERENCES Rentals(RentalId)
);

CREATE TABLE SupportRequests (
    SupportRequestId UNIQUEIDENTIFIER PRIMARY KEY,
    CustomerId UNIQUEIDENTIFIER NOT NULL,
    RentalId UNIQUEIDENTIFIER NULL,
    Subject NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Status NVARCHAR(30) NOT NULL,
    CreatedAtUtc DATETIME2 NOT NULL,
    CONSTRAINT FK_SupportRequests_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(CustomerId),
    CONSTRAINT FK_SupportRequests_Rentals FOREIGN KEY (RentalId) REFERENCES Rentals(RentalId)
);
