using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Griffin.DataSync.Service.Interfaces;
using Griffin.DataSync.Service.Models;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Griffin.DataSync.Service.Services;

public class EmailService : IEmailService
{
    private readonly EmailOptions _options;
    private readonly ILogger<EmailService> _logger;
    private readonly HttpClient _httpClient;

    private IConfidentialClientApplication? _msalClient;

    public EmailService(
        IOptions<EmailOptions> options,
        ILogger<EmailService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("MicrosoftGraph");
    }

    public async Task SendAsync(
        string recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {

        if (string.IsNullOrWhiteSpace(recipients))
        {
            throw new ArgumentException(
                "No email recipients were provided.",
                nameof(recipients));
        }

        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException(
                "Email subject cannot be empty.",
                nameof(subject));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException(
                "Email body cannot be empty.",
                nameof(body));
        }

        var recipientList = ParseRecipients(recipients);

        if (recipientList.Count == 0)
        {
            throw new InvalidOperationException(
                "No valid email recipients were found.");
        }
        _logger.LogInformation(
            "Sending email through Microsoft Graph. From: {Sender}, Recipients: {RecipientCount}, Subject: {Subject}",
            _options.SenderEmail,
            recipientList.Count,
            subject);

        var accessToken = await GetAccessTokenAsync(cancellationToken);

        var graphRequest = new
        {
            message = new
            {
                subject = subject,

                body = new
                {
                    contentType = "HTML",
                    content = body
                },

                toRecipients = recipientList
                    .Select(email => new
                    {
                        emailAddress = new
                        {
                            address = email
                        }
                    })
                    .ToArray()
            },

            saveToSentItems = true
        };

        var json = JsonSerializer.Serialize(
            graphRequest,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"users/{Uri.EscapeDataString(_options.SenderEmail)}/sendMail");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(
            request,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Microsoft Graph email failed. Status: {StatusCode}. Response: {Response}",
                (int)response.StatusCode,
                responseBody);

            throw new InvalidOperationException(
                $"Microsoft Graph email failed with HTTP {(int)response.StatusCode} ({response.StatusCode}). " +
                $"Response: {responseBody}");
        }

        _logger.LogInformation(
            "Email successfully accepted by Microsoft Graph. Subject: {Subject}, Recipients: {RecipientCount}",
            subject,
            recipientList.Count);
    }

    private async Task<string> GetAccessTokenAsync(
        CancellationToken cancellationToken)
    {
        _msalClient ??= ConfidentialClientApplicationBuilder
            .Create(_options.ClientId)
            .WithClientSecret(_options.ClientSecret)
            .WithAuthority(
                $"https://login.microsoftonline.com/{_options.TenantId}")
            .Build();

        var result = await _msalClient
            .AcquireTokenForClient(
                new[]
                {
                    "https://graph.microsoft.com/.default"
                })
            .ExecuteAsync(cancellationToken);

        return result.AccessToken;
    }

    private static List<string> ParseRecipients(
        string recipients)
    {
        return recipients
            .Split(
                new[] { ';', ',', '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

   
}