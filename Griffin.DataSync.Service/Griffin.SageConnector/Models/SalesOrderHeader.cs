namespace Griffin.SageConnector.Models;

public class SalesOrderHeader
{
    public string SalesOrderNo { get; set; } = "";
    public string OrderType { get; set; } = "";
    public DateTime ShipByDate { get; set; }
}