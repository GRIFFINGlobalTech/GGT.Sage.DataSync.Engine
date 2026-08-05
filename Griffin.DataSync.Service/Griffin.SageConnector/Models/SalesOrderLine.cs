namespace Griffin.SageConnector.Models;

public class SalesOrderLine
{
    public string SalesOrderNo { get; set; } = "";
    public string ItemCode { get; set; } = "";
    public decimal QuantityOrdered { get; set; }
}