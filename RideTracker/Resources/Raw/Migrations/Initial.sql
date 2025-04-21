-- Table for Groups
CREATE TABLE Groups (
    Name TEXT NOT NULL,
    Id TEXT PRIMARY KEY,
    CreatedAt BIGINT NOT NULL,
    SynchronizedWithCloudAt BIGINT,
    DeletedAt BIGINT,
    IsCurrent BOOLEAN NOT NULL,
    IsManagedByCurrentUser BOOLEAN NOT NULL,
    IsUploadedToCloud BOOLEAN NOT NULL,
    UpdatedAt BIGINT
);

--------------------------------------------

-- Table for Rides
CREATE TABLE Rides (
    Id TEXT PRIMARY KEY,
    VehicleId TEXT NOT NULL,
    VehicleName TEXT NOT NULL,
    RideDurationInMinutes INTEGER NOT NULL,
    UnitOfTimeInMinutes INTEGER NOT NULL,
    PricePerUnitOfTime INTEGER NOT NULL,
    Cost INTEGER NOT NULL,
    DeletedAt BIGINT,
    CreatedAt BIGINT NOT NULL,
    StartedAt BIGINT NOT NULL,
    StoppedAt BIGINT NOT NULL,
    IsUploadedToCloud BOOLEAN NOT NULL,
    SynchronizedWithCloudAt BIGINT,
    UpdatedAt BIGINT,
    CreatedBy TEXT NOT NULL
);

--------------------------------------------
    
-- Table for Settings
CREATE TABLE Settings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL
);

--------------------------------------------

-- Table for Vehicles
CREATE TABLE Vehicles (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    QuickSaveButtons TEXT NOT NULL,
    PricePerUnitOfTime INTEGER NOT NULL,
    UnitOfTimeInMinutes INTEGER NOT NULL,
    "Order" INTEGER NOT NULL,
    UpdatedAt BIGINT,
    DeletedAt BIGINT,
    CreatedAt BIGINT NOT NULL,
    RideStartedAt BIGINT NOT NULL,
    IsUploadedToCloud BOOLEAN NOT NULL,
    SynchronizedWithCloudAt BIGINT,
    GroupId TEXT NOT NULL,
    FOREIGN KEY (GroupId) REFERENCES Groups(Id)
);