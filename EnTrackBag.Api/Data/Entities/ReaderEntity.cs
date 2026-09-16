namespace EnTrackBag.Api.Data.Entities;

public class ReaderEntity
{
    public int ID { get; set; }
    public string? ReaderCode { get; set; }
    public string? ReaderLocation { get; set; }
    public string? ReaderIP { get; set; }
    public int? ReaderPort { get; set; }
    public DateTime? LastConnected { get; set; }
    public DateTime? LastDisconnected { get; set; }
    public string? LastError { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? ReaderHostName { get; set; }
}
