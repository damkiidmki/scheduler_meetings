using Quartz;
using Quartz.Spi;

namespace ServiceMeetings.JobFactory
{
    class JobFactory : IJobFactory
    {
        private readonly IServiceProvider service;
        public JobFactory(IServiceProvider serviceProvider)
        {
            service = serviceProvider;
        }
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;
            return (IJob)service.GetService(jobDetail.JobType);
        }

        public void ReturnJob(IJob job)
        {
            
        }
    }
}
