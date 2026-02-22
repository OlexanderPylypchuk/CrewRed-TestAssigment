using CrewRed_Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrewRed_Test.Services
{
    public class DuplicateTracker : IDisposable
    {
        private readonly HashSet<string> _keys = new();
        private readonly StreamWriter _duplicateWriter;

        public DuplicateTracker(string duplicateFilePath)
        {
            _duplicateWriter = new StreamWriter(duplicateFilePath);
        }

        public bool IsDuplicate(Trip trip)
        {
            var key = $"{trip.Pickup:o}|{trip.Dropoff:o}|{trip.PassengerCount}";

            if (_keys.Contains(key))
            {
                _duplicateWriter.WriteLine(key);
                return true;
            }

            _keys.Add(key);
            return false;
        }

        public void Dispose()
        {
            _duplicateWriter?.Flush();
            _duplicateWriter?.Dispose();
        }
    }
}
