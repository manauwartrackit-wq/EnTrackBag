namespace EnTrackBag.Api.Data.Entities;

public class SuspectBagEntity
{
    public int ID { get; set; }
    public string TagID { get; set; } = string.Empty;
    public string? GlobalID { get; set; }
    public string? ScanResult { get; set; }
    public DateTime? TStamp { get; set; }
    public int? StationID { get; set; }
    public string? IATACode { get; set; }
    public int? RecheckStationID { get; set; }
    public DateTime? RecheckTime { get; set; }
    public int? LastStage { get; set; }
    public int? ExitGateID { get; set; }
    public bool? IsAlarm { get; set; }
    public DateTime? LastSeenTime { get; set; }
    public int? LastSeenAt { get; set; }
}
