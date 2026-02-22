# CrewRed-Test
The architecture isnt perfect, as it has a magical string for connection, no facade for separating output and architecture, etc. I was purely focusing on the task. As you can see lower on the schema, I created multiple indexes to optimise reads as much as possible/

If it was a large collection, I would remove hashset that stores dupes, and instead let the database handle the duplicates, as for it would be very ram inneficient to store such a large collection. 



#Script to create MSSQL table

CREATE DATABASE CrewRedDb;
USE CrewRedDb;

CREATE TABLE Trips (
    Id BIGINT IDENTITY(1,1) PRIMARY KEY,
    tpep_pickup_datetime DATETIME2(0) NOT NULL,
    tpep_dropoff_datetime DATETIME2(0) NOT NULL,
    passenger_count TINYINT NOT NULL,
    trip_distance DECIMAL(9,3) NOT NULL,
    store_and_fwd_flag VARCHAR(3) NOT NULL,
    PULocationID INT NOT NULL,
    DOLocationID INT NOT NULL,
    fare_amount DECIMAL(10,2) NOT NULL,
    tip_amount DECIMAL(10,2) NOT NULL
);

CREATE INDEX IX_Trips_PULocationID ON Trips(PULocationID);
CREATE INDEX IX_Trips_TripDistance ON Trips(trip_distance DESC);
CREATE INDEX IX_Trips_PickupDropoff 
ON Trips(tpep_pickup_datetime, tpep_dropoff_datetime);
