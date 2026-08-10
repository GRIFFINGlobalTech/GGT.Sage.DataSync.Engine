using Griffin.DataSync.Service.Interfaces;

namespace Griffin.DataSync.Service.Jobs;

public class UpdateShipperBoardJob : ISyncJob
{
    private readonly ISqlRepo _sqlRepo;
    private readonly ILogger<UpdateShipperBoardJob> _logger;

    public string JobName => "Refresh Griffin Shippers Board";

    public TimeSpan Interval => TimeSpan.FromMinutes(5);

    public UpdateShipperBoardJob(
        ISqlRepo sqlRepo,
        ILogger<UpdateShipperBoardJob> logger)
    {
        _sqlRepo = sqlRepo;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        const string procedure =
            "dbo.usp_RefreshGriffinShippersBoard";

        _logger.LogInformation(
            "Starting {Job}",
            JobName);

        try
        {
            await _sqlRepo.ExecuteProcedureAsync(
                procedure,
                cancellationToken);

            _logger.LogInformation(
                "{Job} completed successfully.",
                JobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "{Job} failed.",
                JobName);

            throw;
        }
    }
}