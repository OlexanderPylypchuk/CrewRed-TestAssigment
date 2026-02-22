using CrewRed_Test.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewRed_Test.Services
{
    public class CsvProcessor
    {
        private readonly DuplicateTracker _duplicateTracker;

        public CsvProcessor(DuplicateTracker duplicateTracker)
        {
            _duplicateTracker = duplicateTracker;
        }

        public IEnumerable<Trip> Process(string filepath)
        {
            using var reader = new StreamReader(filepath);

            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
                yield break;

            var headerMap = BuildHeaderMap(headerLine);

            string[] requiredColumns =
            {
                "tpep_pickup_datetime",
                "tpep_dropoff_datetime",
                "passenger_count",
                "trip_distance",
                "store_and_fwd_flag",
                "PULocationID",
                "DOLocationID",
                "fare_amount",
                "tip_amount"
            };

            foreach (var column in requiredColumns)
            {
                if (!headerMap.ContainsKey(column))
                    throw new InvalidOperationException($"Missing required column: {column}");
            }

            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = line.Split('\t');

                if (values.Length != headerMap.Count)
                    continue; 

                if (!TryCreateTrip(values, headerMap, est, out var trip))
                    continue;

                if (_duplicateTracker.IsDuplicate(trip))
                    continue;

                yield return trip;
            }
        }

        private static Dictionary<string, int> BuildHeaderMap(string headerLine)
        {
            var headers = headerLine.Split('\t');

            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < headers.Length; i++)
            {
                var columnName = headers[i].Trim();
                if (!map.ContainsKey(columnName))
                    map[columnName] = i;
            }

            return map;
        }

        private static bool TryCreateTrip(
            string[] values,
            Dictionary<string, int> headerMap,
            TimeZoneInfo est,
            out Trip trip)
        {
            trip = null;

            try
            {
                if (!DateTime.TryParse(values[headerMap["tpep_pickup_datetime"]].Trim(),
                    out var pickupEst))
                    return false;

                if (!DateTime.TryParse(values[headerMap["tpep_dropoff_datetime"]].Trim(),
                    out var dropoffEst))
                    return false;

                if (!byte.TryParse(values[headerMap["passenger_count"]].Trim(),
                    out var passengerCount))
                    return false;

                if (!decimal.TryParse(
                        values[headerMap["trip_distance"]].Trim(),
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var tripDistance))
                    return false;

                if (!int.TryParse(values[headerMap["PULocationID"]].Trim(),
                    out var puLocation))
                    return false;

                if (!int.TryParse(values[headerMap["DOLocationID"]].Trim(),
                    out var doLocation))
                    return false;

                if (!decimal.TryParse(
                        values[headerMap["fare_amount"]].Trim(),
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var fareAmount))
                    return false;

                if (!decimal.TryParse(
                        values[headerMap["tip_amount"]].Trim(),
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var tipAmount))
                    return false;

                var pickupUtc = TimeZoneInfo.ConvertTimeToUtc(pickupEst, est);
                var dropoffUtc = TimeZoneInfo.ConvertTimeToUtc(dropoffEst, est);

                var storeFlagRaw = values[headerMap["store_and_fwd_flag"]].Trim();
                var storeFlag = storeFlagRaw switch
                {
                    "Y" => "Yes",
                    "N" => "No",
                    _ => "No"
                };

                trip = new Trip
                {
                    Pickup = pickupUtc,
                    Dropoff = dropoffUtc,
                    PassengerCount = passengerCount,
                    TripDistance = tripDistance,
                    StoreAndFwdFlag = storeFlag,
                    PULocationID = puLocation,
                    DOLocationID = doLocation,
                    FareAmount = fareAmount,
                    TipAmount = tipAmount
                };

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
