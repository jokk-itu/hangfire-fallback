using Api;
using Api.Filters;
using Hangfire;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddHangfire(configuration =>
    {
        configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180);
        configuration.UseSimpleAssemblyNameTypeSerializer();
        configuration.UseRecommendedSerializerSettings();
        configuration.UseSqlServerStorage("Server=localhost;Database=HangfireTest;User ID=sa;Password=Password12!;TrustServerCertificate=true");
    });

builder.Services.AddScoped<IFireAndForgetJob<SuccessContext>, SuccessJob>();
builder.Services.AddScoped<IFireAndForgetJob<FailContext>, FailJob>();

builder.Services.AddHangfireServer(configuration =>
{
    configuration.Queues =
        [
            QueueConstants.Priority10,
            QueueConstants.Priority20,
            QueueConstants.Priority30,
            QueueConstants.Priority40,
            QueueConstants.Priority50,
            QueueConstants.Priority50Fallback,
            QueueConstants.Priority60,
            QueueConstants.Priority70,
            QueueConstants.Priority80,
            QueueConstants.Priority90,
            QueueConstants.Priority90Fallback
        ];
});

// Remove as the Retry logic is handled manually through custom filters
GlobalJobFilters.Filters.Remove<AutomaticRetryAttribute>();

// Add Custom filters
GlobalJobFilters.Filters.Add(new RetryFilter());
GlobalJobFilters.Filters.Add(new QueueFilter());

var app = builder.Build();

app.UseHangfireDashboard();

app.MapGet(
    "enqueue-success",
    ([FromServices] IBackgroundJobClient backgroundJobClient, [FromServices] IFireAndForgetJob<SuccessContext> dummyJob, CancellationToken cancellationToken) =>
    {
        backgroundJobClient.Enqueue(() => dummyJob.Execute(new SuccessContext{ Id = Guid.NewGuid() }, cancellationToken));
    });

app.MapGet(
    "enqueue-fail",
    ([FromServices] IBackgroundJobClient backgroundJobClient, [FromServices] IFireAndForgetJob<FailContext> dummyJob, CancellationToken cancellationToken) =>
    {
        backgroundJobClient.Enqueue(() => dummyJob.Execute(new FailContext { Id = Guid.NewGuid() }, cancellationToken));
    });

app.Run();