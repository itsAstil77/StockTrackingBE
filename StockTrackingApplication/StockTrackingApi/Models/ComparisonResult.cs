public class ComparisonResult
{
    public string ProductCode { get; set; }
    public int File1Qty { get; set; }
    public int File2Qty { get; set; }
    public int Difference => File2Qty - File1Qty;
    public string Status { get; set; }
}
