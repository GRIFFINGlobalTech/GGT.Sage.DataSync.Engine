namespace Griffin.DataSync.Service.Models;

public class TableSyncDefinition
{
    public string Name { get; set; } = "";

    public string ConnectorCommand { get; set; } = "";

    public string StageTable { get; set; } = "";

    public string MergeProcedure { get; set; } = "";
}