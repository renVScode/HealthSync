using HealthSync.Core.Interfaces.Services;

namespace HealthSync.Api.Services;

public class AppointmentNoShowService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentNoShowService> _logger;

    public AppointmentNoShowService(IServiceScopeFactory scopeFactory, ILogger<AppointmentNoShowService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AppointmentNoShowService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var appointmentService = scope.ServiceProvider.GetRequiredService<IAppointmentService>();
                var count = await appointmentService.MarkNoShowAppointmentsAsync();
                if (count > 0)
                    _logger.LogInformation("Marked {Count} appointment(s) as NoShow", count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while marking no-show appointments");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
