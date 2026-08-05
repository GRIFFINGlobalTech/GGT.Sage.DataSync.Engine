namespace Griffin.SageConnector.Models;

public class InventoryBin
{
    public string ItemCode { get; set; } = "";
    public decimal QuantityOnHand { get; set; }
    public string ItemDesc { get; set; } = "";
}