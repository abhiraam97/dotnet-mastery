namespace ShopSystem.Models;

/// <summary>
/// One measurement from one sensor at one moment.
/// Kept deliberately generic so any sensor (temperature, door, ...) fits.
/// </summary>
public class Reading
{
    public int Id { get; set; }                       // primary key (EF fills this in)
    public string SensorId { get; set; } = "";        // e.g. "fridge-1", "front-door"
    public string Metric { get; set; } = "";          // e.g. "temperature_c", "door_open"
    public double Value { get; set; }                 // 4.5, or 1/0 for open/closed
    public DateTime TimestampUtc { get; set; }        // set by the server on ingest
}
