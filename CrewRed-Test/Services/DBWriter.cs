using CrewRed_Test.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewRed_Test.Services
{
    public class DBWriter
    {
        private readonly string _connectionString;

        public DBWriter(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task BulkInsertAsync(IEnumerable<Trip> trips)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            using var bulkCopy = new SqlBulkCopy(connection)
            {
                DestinationTableName = "Trips",
                BatchSize = 10000
            };

            var table = CreateDataTable();

            bulkCopy.ColumnMappings.Add("tpep_pickup_datetime", "tpep_pickup_datetime");
            bulkCopy.ColumnMappings.Add("tpep_dropoff_datetime", "tpep_dropoff_datetime");
            bulkCopy.ColumnMappings.Add("passenger_count", "passenger_count");
            bulkCopy.ColumnMappings.Add("trip_distance", "trip_distance");
            bulkCopy.ColumnMappings.Add("store_and_fwd_flag", "store_and_fwd_flag");
            bulkCopy.ColumnMappings.Add("PULocationID", "PULocationID");
            bulkCopy.ColumnMappings.Add("DOLocationID", "DOLocationID");
            bulkCopy.ColumnMappings.Add("fare_amount", "fare_amount");
            bulkCopy.ColumnMappings.Add("tip_amount", "tip_amount");

            foreach (var trip in trips)
            {
                table.Rows.Add(
                    trip.Pickup,
                    trip.Dropoff,
                    trip.PassengerCount,
                    trip.TripDistance,
                    trip.StoreAndFwdFlag,
                    trip.PULocationID,
                    trip.DOLocationID,
                    trip.FareAmount,
                    trip.TipAmount
                );

                if (table.Rows.Count >= 10000)
                {
                    await bulkCopy.WriteToServerAsync(table);
                    table.Clear();
                }
            }

            if (table.Rows.Count > 0)
                await bulkCopy.WriteToServerAsync(table);
        }

        private DataTable CreateDataTable()
        {
            var table = new DataTable();
            table.Columns.Add("tpep_pickup_datetime", typeof(DateTime));
            table.Columns.Add("tpep_dropoff_datetime", typeof(DateTime));
            table.Columns.Add("passenger_count", typeof(byte));
            table.Columns.Add("trip_distance", typeof(decimal));
            table.Columns.Add("store_and_fwd_flag", typeof(string));
            table.Columns.Add("PULocationID", typeof(int));
            table.Columns.Add("DOLocationID", typeof(int));
            table.Columns.Add("fare_amount", typeof(decimal));
            table.Columns.Add("tip_amount", typeof(decimal));

            return table;
        }
    }
}
