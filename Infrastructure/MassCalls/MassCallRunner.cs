using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using NiquiBackend.Application.Interfaces.Services;
using NiquiBackend.Infrastructure.Common;
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

    public void Start(Guid executionId, string convenio, int targetCalls, int timeLimitMinutes, string timeSlot)
    {
        var cts = new CancellationTokenSource(TimeSpan.FromMinutes(timeLimitMinutes));
        _running[executionId] = cts;
        _ = Task.Run(() => RunAsync(executionId, convenio, targetCalls, timeSlot, cts.Token), cts.Token);
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

    private async Task RunAsync(Guid executionId, string convenio, int targetCalls, string timeSlot, CancellationToken token)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NiquiDbContext>();
        var customerService = scope.ServiceProvider.GetRequiredService<ICustomerService>();

        var execution = await db.MassCallExecutions.FindAsync(new object[] { executionId }, token);
        if (execution == null) return;

        int callsMade = 0;

        try
        {
            var customers = await customerService.GetEligibleForMassCallAsync(convenio);
            var fromNumber = _config["Twilio:PhoneNumber"]!;
            var baseUrl = _config["PublicBaseUrl"]!.TrimEnd('/');

            foreach (var customer in customers)
            {
                if (token.IsCancellationRequested) break;
                if (callsMade >= targetCalls) break;

                // Control de franja horaria en hora Colombia
                bool isInValidTimeSlot = await EnsureValidTimeSlotAsync(timeSlot, token);
                if (!isInValidTimeSlot) break;

                try
                {
                    var formattedPhone = NormalizeColombianNumber(customer.PhoneNumber);

                    CallResource.Create(
                        to: new PhoneNumber(formattedPhone),
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

    private async Task<bool> EnsureValidTimeSlotAsync(string timeSlot, CancellationToken token)
    {
        var slot = timeSlot?.ToLowerInvariant().Trim() ?? "";

        while (!token.IsCancellationRequested)
        {
            var nowCol = ColombiaTimeHelper.Now;
            var currentTime = nowCol.TimeOfDay;

            var morningStart   = new TimeSpan(9, 0, 0);   // 09:00 AM
            var morningEnd     = new TimeSpan(12, 0, 0);  // 12:00 PM
            var afternoonStart = new TimeSpan(14, 0, 0); // 02:00 PM
            var afternoonEnd   = new TimeSpan(17, 0, 0);  // 05:00 PM

            if (slot == "manana" || slot == "morning")
            {
                if (currentTime < morningStart)
                {
                    await Task.Delay(morningStart - currentTime, token);
                    continue;
                }
                if (currentTime >= morningEnd) return false;
                return true;
            }

            if (slot == "tarde" || slot == "afternoon")
            {
                if (currentTime < afternoonStart)
                {
                    await Task.Delay(afternoonStart - currentTime, token);
                    continue;
                }
                if (currentTime >= afternoonEnd) return false;
                return true;
            }

            // Franja "todo_el_dia" / "allday" / por defecto
            if (currentTime < morningStart)
            {
                await Task.Delay(morningStart - currentTime, token);
                continue;
            }

            if (currentTime >= morningEnd && currentTime < afternoonStart)
            {
                // Pausa automática de 12:00 PM a 2:00 PM (reanuda sola a las 2:00 PM)
                await Task.Delay(afternoonStart - currentTime, token);
                continue;
            }

            if (currentTime >= afternoonEnd) return false;

            return true;
        }

        return false;
    }

    private static string NormalizeColombianNumber(string rawPhone)
    {
        if (string.IsNullOrWhiteSpace(rawPhone)) return string.Empty;

        var cleaned = new string(rawPhone.Where(char.IsDigit).ToArray());

        if (cleaned.StartsWith("0057")) cleaned = cleaned[4..];
        else if (cleaned.StartsWith("57") && cleaned.Length > 10) cleaned = cleaned[2..];
        else if (cleaned.StartsWith("0") && cleaned.Length == 11) cleaned = cleaned[1..];

        return $"+57{cleaned}";
    }
}