namespace EnTrackBag.Api.Data.Entities;

public class TagViewEntity
{
    public int AlarmID { get; set; }
    public string? TagID { get; set; }
    public DateTime? TStamp { get; set; }
    public string? AlarmDesc { get; set; }
    public string? AlarmType { get; set; }
    public bool? IsReported { get; set; }
    public bool? IsAlarm { get; set; }
    public bool? IsDelayed { get; set; }
    public int? Stage { get; set; }
    public string? GlobalID { get; set; }
    public string? ScanResult { get; set; }
    public int? LastStage { get; set; }
    public bool? IsTagAlarm { get; set; }
    public DateTime? LastSeenTime { get; set; }
    public string? LogicalDeviceCode { get; set; }
    public string? CurrentAlarmLocation { get; set; }
    public bool? IsCancelled { get; set; }
    public DateTime? AlarmTime { get; set; }
    public string? IATACode { get; set; }
}
