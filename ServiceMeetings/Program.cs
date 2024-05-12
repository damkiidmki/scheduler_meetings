using Itenso.TimePeriod;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using ServiceMeetings.JobFactory;
using ServiceMeetings.Jobs;
using ServiceMeetings.Models;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IJobFactory, JobFactory>();
                services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();                    

                services.AddSingleton<NotificationJob>();
        

                #region Adding Jobs 

                var meetings = new List<Meeting>();
                var timePeriod = new TimePeriodCollection();

                services.AddSingleton(timePeriod);
                services.AddSingleton(meetings);
                #endregion
                
                services.AddHostedService<ServiceMeetings.Service.ServiceMeetings>();

            });


}