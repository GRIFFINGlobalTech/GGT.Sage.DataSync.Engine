using System.Data;
using System.Diagnostics;
using System.Text.Json;

namespace Griffin.DataSync.Service.Services;

public class SageConnectorRunner
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SageConnectorRunner> _logger;

    public SageConnectorRunner(
        IConfiguration configuration,
        ILogger<SageConnectorRunner> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // Existing method (leave it)
    public async Task<List<T>> ExecuteAsync<T>(string command)
    {
        var json = await ExecuteConnectorAsync(command);

        return JsonSerializer.Deserialize<List<T>>(json)
               ?? new List<T>();
    }

    // New method
    public async Task<DataTable> ExecuteDataTableAsync(string command)
    {
        var json = await ExecuteConnectorAsync(command);

        return JsonToDataTable(json);
    }

    // Common connector execution
    private async Task<string> ExecuteConnectorAsync(string command)
    {
        var process = new Process();

        process.StartInfo.FileName =
            _configuration["SageConnector:Path"]!;

        process.StartInfo.Arguments = command;

        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        _logger.LogInformation(
            "Launching connector: {Path}",
            process.StartInfo.FileName);

        _logger.LogInformation(
            "Arguments: {Args}",
            process.StartInfo.Arguments);

        process.Start();

        string json = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new Exception(error);

        return json;
    }

    private static DataTable JsonToDataTable(string json)
    {
        var table = new DataTable();

        using var document = JsonDocument.Parse(json);

        if (document.RootElement.ValueKind != JsonValueKind.Array)
            return table;

        var rows = document.RootElement;

        if (rows.GetArrayLength() == 0)
            return table;

        // Create columns
        foreach (var property in rows[0].EnumerateObject())
        {
            table.Columns.Add(property.Name);
        }

        // Add rows
        foreach (var item in rows.EnumerateArray())
        {
            var row = table.NewRow();

            foreach (var property in item.EnumerateObject())
            {
                row[property.Name] =
                    property.Value.ValueKind == JsonValueKind.Null
                        ? DBNull.Value
                        : property.Value.ToString();
            }

            table.Rows.Add(row);
        }

        return table;
    }
}