using Griffin.DataSync.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Griffin.DataSync.Service.Jobs
{
    public class SendEmailAlertsJob : ISyncJob
    {
        private readonly ISqlRepo _sqlRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<SendEmailAlertsJob> _logger;

        public string JobName =>
            "Send Email Alerts";

        public TimeSpan Interval =>
            TimeSpan.FromMinutes(1);

        public SendEmailAlertsJob(
            ISqlRepo sqlRepo,
            IEmailService emailService,
            ILogger<SendEmailAlertsJob> logger)
        {
            _sqlRepo = sqlRepo;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task ExecuteAsync(
            CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            var stopwatch =
                System.Diagnostics.Stopwatch.StartNew();

            _logger.LogInformation(
                "Starting {Job} at {StartTime}",
                JobName,
                startTime);

            try
            {
                var emails =
                    await _sqlRepo.GetPendingEmailsAsync(
                        cancellationToken);

                _logger.LogInformation(
                    "Found {Count} pending emails",
                    emails.Count);

                foreach (var email in emails)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        await _emailService.SendAsync(
                            email.Recipients,
                            email.Subject,
                            email.Body,
                            cancellationToken);

                        await _sqlRepo.MarkEmailSentAsync(
                            email.ID,
                            cancellationToken);

                        _logger.LogInformation(
                            "Email {EmailID} sent successfully. Reference: {ReferenceNo}",
                            email.ID,
                            email.ReferenceNo);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            ex,
                            "Failed to send email {EmailID}. Reference: {ReferenceNo}",
                            email.ID,
                            email.ReferenceNo);

                        try
                        {
                            await _sqlRepo.MarkEmailFailedAsync(
                                email.ID,
                                ex.Message,
                                cancellationToken);
                        }
                        catch (Exception logEx)
                        {
                            _logger.LogError(
                                logEx,
                                "Failed to mark email {EmailID} as failed",
                                email.ID);
                        }
                    }
                }

                stopwatch.Stop();

                var endTime = DateTime.Now;

                var durationMs =
                    stopwatch.ElapsedMilliseconds;

                _logger.LogInformation(
                    "{Job} completed at {EndTime}. Duration: {DurationMs} ms ({DurationSeconds:F2} seconds)",
                    JobName,
                    endTime,
                    durationMs,
                    stopwatch.Elapsed.TotalSeconds);

                await _sqlRepo.LogJobExecutionAsync(
                    JobName,
                    startTime,
                    endTime,
                    durationMs,
                    "SUCCESS",
                    null,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                var endTime = DateTime.Now;

                var durationMs =
                    stopwatch.ElapsedMilliseconds;

                _logger.LogError(
                    ex,
                    "{Job} failed after {DurationMs} ms",
                    JobName,
                    durationMs);

                try
                {
                    await _sqlRepo.LogJobExecutionAsync(
                        JobName,
                        startTime,
                        endTime,
                        durationMs,
                        "FAILED",
                        ex.Message,
                        cancellationToken);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(
                        logEx,
                        "Failed to write execution history for {Job}",
                        JobName);
                }

                throw;
            }
        }
    }
}
