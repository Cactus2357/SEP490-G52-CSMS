using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SEP490_G52_CSMS.Repositories.Interfaces;

namespace SEP490_G52_CSMS.Services
{
    /// <summary>
    /// Background service tự động kiểm tra và đóng các ca làm việc còn đang mở (Active) từ các ngày trước khi bước sang ngày mới
    /// </summary>
    public class ShiftAutoCloseBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ShiftAutoCloseBackgroundService> _logger;
        private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(15);

        public ShiftAutoCloseBackgroundService(IServiceProvider serviceProvider, ILogger<ShiftAutoCloseBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ShiftAutoCloseBackgroundService đã khởi chạy.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repository = scope.ServiceProvider.GetRequiredService<ICashHandoverRepository>();
                    var closedCount = await repository.AutoCloseStaleActiveHandoversAsync();
                    if (closedCount > 0)
                    {
                        _logger.LogInformation("Tự động đóng {Count} ca làm việc còn mở từ ngày trước khi bước sang ngày mới.", closedCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi tự động đóng ca quá hạn trong ShiftAutoCloseBackgroundService.");
                }

                try
                {
                    await Task.Delay(CheckInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}
