using CrewRed_Test.Services;


var csvPath = "input.csv";
var duplicatesPath = "duplicates.csv";

using var duplicateTracker = new DuplicateTracker(duplicatesPath);
var processor = new CsvProcessor(duplicateTracker);
var dbWriter = new DBWriter("Data Source=localhost\\SQLEXPRESS;Initial Catalog=CrewRedDb;TrustServerCertificate=True;Trusted_Connection=True;");

var trips = processor.Process(csvPath);

await dbWriter.BulkInsertAsync(trips);

Console.WriteLine("ETL completed.");