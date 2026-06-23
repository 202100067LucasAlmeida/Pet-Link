using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PetLink.Services; 
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PetLink.Hubs 
{
    public class EventReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EventReminderService> _logger;

        public EventReminderService(IServiceProvider serviceProvider, ILogger<EventReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        await notificationService.SendEventReminderNotificationsAsync();
                        _logger.LogInformation("Event reminder notifications sent at {Time}", DateTime.Now);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending event reminder notifications");
                }

                // Verificar a cada hora
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }
}