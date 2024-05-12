using Microsoft.Extensions.Logging;
using Quartz;

namespace ServiceMeetings.Jobs
{
    class NotificationJob : IJob
    {
        private readonly ILogger<NotificationJob> _logger;
        public NotificationJob(ILogger<NotificationJob> logger)
        {
            this._logger = logger;
        }
        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"{context.Trigger.Description}");
            return Task.CompletedTask;
        }
    }
}
