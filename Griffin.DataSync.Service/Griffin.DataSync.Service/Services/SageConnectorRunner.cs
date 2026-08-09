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

    public async Task<List<T>> ExecuteAsync<T>(
        string command)
    {
        var json =
            await ExecuteConnectorAsync(command);

        return JsonSerializer.Deserialize<List<T>>(json)
               ?? new List<T>();
    }

    public async Task<DataTable> ExecuteDataTableAsync(
        string command)
    {
        var json =
            await ExecuteConnectorAsync(command);

        return JsonToDataTable(json);
    }

    private async Task<string> ExecuteConnectorAsync(
        string command)
    {
        using var process = new Process();

        process.StartInfo.FileName =
            _configuration["SageConnector:Path"]!;

        process.StartInfo.Arguments =
            command;

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

        var jsonTask =
            process.StandardOutput.ReadToEndAsync();

        var errorTask =
            process.StandardError.ReadToEndAsync();

        await Task.WhenAll(
            jsonTask,
            errorTask);

        await process.WaitForExitAsync();

        var json = await jsonTask;
        var error = await errorTask;

        if (process.ExitCode != 0)
        {
            throw new Exception(
                string.IsNullOrWhiteSpace(error)
                    ? $"Connector exited with code {process.ExitCode}."
                    : error);
        }

        return json;
    }

    private DataTable JsonToDataTable(
        string json)
    {
        var table = new DataTable();

        using var document =
            JsonDocument.Parse(json);

        if (document.RootElement.ValueKind !=
            JsonValueKind.Array)
        {
            throw new Exception(
                "Connector response is not a JSON array.");
        }

        var rows =
            document.RootElement;

        if (rows.GetArrayLength() == 0)
        {
            return table;
        }

        var firstRow =
            rows[0];

        if (firstRow.ValueKind !=
            JsonValueKind.Object)
        {
            throw new Exception(
                "Connector returned an invalid row format.");
        }

        // =========================================================
        // CREATE COLUMNS
        // =========================================================

        foreach (var property in
                 firstRow.EnumerateObject())
        {
            var columnName =
                property.Name.Trim();

            // This should NEVER happen with the correct
            // connector JSON, but protects us from accidentally
            // creating a column containing comma-separated names.
            if (columnName.Contains(','))
            {
                throw new Exception(
                    $"Invalid connector column name detected: '{columnName}'. " +
                    "The connector is returning multiple column names as one property.");
            }

            if (!table.Columns.Contains(columnName))
            {
                table.Columns.Add(columnName);
            }
        }


        // =========================================================
        // CREATE ROWS
        // =========================================================

        foreach (var item in rows.EnumerateArray())
        {
            var dataRow =
                table.NewRow();

            foreach (var property in
                     item.EnumerateObject())
            {
                var columnName =
                    property.Name.Trim();

                if (!table.Columns.Contains(columnName))
                    continue;

                dataRow[columnName] =
                    property.Value.ValueKind ==
                    JsonValueKind.Null
                        ? DBNull.Value
                        : property.Value.ToString();
            }

            table.Rows.Add(dataRow);
        }

        return table;
    }
}