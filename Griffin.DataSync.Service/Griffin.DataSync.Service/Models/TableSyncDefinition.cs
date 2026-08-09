namespace Griffin.DataSync.Service.Models;

public class TableSyncDefinition
{
    public string JobName { get; set; } = string.Empty;

    public string Command { get; set; } = string.Empty;

    public string StageTable { get; set; } = string.Empty;

    public string MergeProcedure { get; set; } = string.Empty;

    public int IntervalMinutes { get; set; }
}