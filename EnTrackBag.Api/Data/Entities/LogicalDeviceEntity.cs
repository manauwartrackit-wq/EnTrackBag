namespace EnTrackBag.Api.Data.Entities;

public class LogicalDeviceEntity
{
    public int ID { get; set; }
    public string? LogicalDeviceCode { get; set; }
    public string? DeviceName { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? DeviceType { get; set; }
    public int? NextDevicePoint { get; set; }
    public string? IPAddress { get; set; }
    public DateTime? LastConnected { get; set; }
    public DateTime? LastDisconnected { get; set; }
    public string? LastError { get; set; }
}
