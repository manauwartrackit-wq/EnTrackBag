namespace EnTrackBag.Api.Data.Entities;

public class LogicalDeviceMapEntity
{
    public int ID { get; set; }
    public int? LogicalDeviceID { get; set; }
    public int? ReaderID { get; set; }
    public int? AntennaID { get; set; }
    public bool? IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public int? IsInOut { get; set; }
}
