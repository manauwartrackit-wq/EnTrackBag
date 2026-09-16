namespace EnTrackBag.Api.Data.Entities;

public class DeviceLogViewEntity
{
    public int AlarmID { get; set; }
    public string? TagID { get; set; }
    public DateTime? TStamp { get; set; }
    public DateTime? AlarmTime { get; set; }
    public bool? IsCancelled { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }
    public string? AlarmDesc { get; set; }
    public string? AlarmType { get; set; }
    public int? LogicalDeviceID { get; set; }
    public bool? IsReported { get; set; }
    public DateTime? ReportedTime { get; set; }
    public string? ResponseError { get; set; }
    public bool? IsAlarm { get; set; }
    public bool? IsDelayed { get; set; }
    public int? Stage { get; set; }
    public string? LogicalDeviceCode { get; set; }
    public string? ReaderIP { get; set; }
    public DateTime? LastConnected { get; set; }
    public int? DeviceType { get; set; }
}
