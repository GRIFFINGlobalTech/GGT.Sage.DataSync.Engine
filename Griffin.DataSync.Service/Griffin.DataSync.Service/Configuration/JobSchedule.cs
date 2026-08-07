namespace Griffin.DataSync.Service.Configuration;

public class JobSchedule
{
    public string JobName { get; set; } = "";

    public TimeSpan Interval { get; set; }

    public DateTime LastRun { get; set; } = DateTime.MinValue;
}