// Infrastructure/MassCalls/MassCallRunner.cs
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Persistence.Generated;
using Twilio.Exceptions;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using HttpMethod = Twilio.Http.HttpMethod;

namespace NiquiBackend.Infrastructure.MassCalls;

public class MassCallRunner
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _running = new();
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;

    public MassCallRunner(IServiceScopeFactory scopeFactory, IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _config = config;
    }

    public void Start(Guid executionId, string convenio, int targetCalls, int timeLimitMinutes)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(timeLimitMinutes));
        _running[executionId] = cts;
        _ = Task.Run(() => RunAsync(executionId, convenio, targetCalls, cts.Token), cts.Token);
    }

    public async Task StopAsync(Guid executionId, NiquiDbContext db)
    {
        if (_running.TryGetValue(executionId, out var cts))
        {
            var execution = await db.MassCallExecutions.FindAsync(executionId);
            if (execution != null && execution.Status == "Running")
            {
                execution.Status = "Stopped";
                await db.SaveChangesAsync();
            }
            cts.Cancel();
        }
    }

    private async Task RunAsync(Guid executionId, string convenio, int targetCalls, CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NiquiDbContext>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var execution = await db.MassCallExecutions.FindAsync(new object[] { executionId }, token);
        if (execution == null) return;

        int callsMade = 0;
        var wasCancelled = false;

        try
        {
            var customers = await customerService.GetEligibleForMassCallAsync(convenio);
            var fromNumber = _config["Twilio:PhoneNumber"]!;
            var baseUrl = _config["PublicBaseUrl"]!.TrimEnd('/');

            foreach (var customer in customers)
            {
                if (token.IsCancellationRequested) { wasCancelled = true; break; }
                if (callsMade >= targetCalls) break;

                try
                {
                    CallResource.Create(
                        to: new PhoneNumber(customer.PhoneNumber),
                        from: new PhoneNumber(fromNumber),
                        url: new Uri($"{baseUrl}/api/calls/twiml/{customer.CustomerId}"),
                        method: HttpMethod.Get,
                        statusCallback: new Uri($"{baseUrl}/api/calls/status-callback?customerId={customer.CustomerId}"),
                        statusCallbackMethod: HttpMethod.Post,
                        statusCallbackEvent: new List<string> { "completed" }
                    );

                    callsMade++;
                    execution.CallsMade = callsMade;
                    await db.SaveChangesAsync(CancellationToken.None);
                }
                catch (ApiException ex) when (ex.Code == 21211 || ex.Code == 21608 || ex.Code == 21614)
                {
                    execution.SkippedUnverified++;
                    await db.SaveChangesAsync(CancellationToken.None);
                    continue;
                }

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(20), token);
                }
                catch (TaskCanceledException)
                {
                    wasCancelled = true;
                    break;
                }
            }

            if (execution.Status != "Stopped")
                execution.Status = callsMade >= targetCalls ? "Completed" : "Timeout";
        }
        finally
        {
            execution.FinishedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(CancellationToken.None);
            _running.TryRemove(executionId, out _);
        }
    }
}