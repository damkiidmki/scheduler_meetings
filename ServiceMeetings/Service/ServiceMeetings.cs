using Itenso.TimePeriod;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using ServiceMeetings.Jobs;
using ServiceMeetings.Models;

namespace ServiceMeetings.Service
{
    class ServiceMeetings : IHostedService
    {
        public IScheduler Scheduler { get; set; }
        private readonly IJobFactory jobFactory;
        private readonly List<Meeting> meetings;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly TimePeriodCollection timePeriods;
        private const string path = "meetings.txt";

        public ServiceMeetings(ISchedulerFactory schedulerFactory,List<Meeting> meetings,IJobFactory jobFactory, TimePeriodCollection timePeriods)
        {
            this.jobFactory = jobFactory;
            this.timePeriods = timePeriods;
            this.schedulerFactory = schedulerFactory;
            this.meetings = meetings;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await schedulerFactory.GetScheduler();
            Scheduler.JobFactory = jobFactory;

            await Scheduler.Start(cancellationToken);


            while (true)
            {
                Info();
                try
                {
                    var selectedCase = int.Parse(Console.ReadLine());
                    int selectedIndex = 0;
                    var meeting = new Meeting();
                    var selectedDate = new DateTime();
                    switch (selectedCase)
                    {
                        case 1:
                            meeting = SetMeeting();
                            ScheduleMeeting(meeting);
                            break;
                        case 2:
                            GetAllMeetings();
                            Console.WriteLine("Выберите встречу, которую хотите изменить ");
                            selectedIndex = int.Parse(Console.ReadLine());
                            meeting = SetMeeting();
                            timePeriods.Remove(meetings[selectedIndex - 1].TimePeriod);
                            UpdateMeeting(meetings[selectedIndex - 1], meeting);
                            break;
                        case 3:
                            GetAllMeetings();
                            Console.WriteLine("Выберите встречу, которую хотите удалить ");
                            selectedIndex = int.Parse(Console.ReadLine());
                            DeleteMeeting(meetings[selectedIndex - 1]);
                            break;
                        case 4:
                            Console.WriteLine("Введите дату");
                            selectedDate = DateTime.Parse(Console.ReadLine());
                            ConsoleWriteMeetingsForTheSelectedDay(selectedDate);
                            break;
                        case 5:
                            Console.WriteLine("Введите дату");
                            selectedDate = DateTime.Parse(Console.ReadLine());
                            PrinttToFileMeetingsForTheSelectedDay(selectedDate);
                            break;

                    }
                }
                catch
                {
                    Console.WriteLine("Что-то пошло не так, повторите операцию");
                }
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler.Shutdown();
        }

        private void ConsoleWriteMeetingsForTheSelectedDay(DateTime selectedDate)
        {
            GetMeetingsForTheSelectedDay(selectedDate)
                .ForEach(x => Console.WriteLine(x));
        }

        private void PrinttToFileMeetingsForTheSelectedDay(DateTime selectedDate)
        {
            using (StreamWriter writer = new StreamWriter(path, false))
            {
                GetMeetingsForTheSelectedDay(selectedDate)
                    .ForEach(x => writer.WriteLineAsync(x.ToString()));
               
            }
        }

        private List<Meeting> GetMeetingsForTheSelectedDay(DateTime selectedDate)
        {
           return meetings.Where(x => x.TimePeriod.Start.Day == selectedDate.Day &&
                x.TimePeriod.Start.Month == selectedDate.Month &&
                x.TimePeriod.Start.Year == selectedDate.Year)
               .ToList();
        }

        private void GetAllMeetings()
        {
            int i = 1;
            meetings?.ForEach(x =>
                Console.WriteLine($"{i++}: {x}"));
        }

        private void UpdateMeeting(Meeting oldMeeting, Meeting newMeeting)
        {
            if (!СheckForIntersectionOfTimeIntervals(newMeeting.TimePeriod))
            {
                DeleteMeeting(oldMeeting);

                SetMeeting(newMeeting);
            }
            else
            {
                Console.WriteLine("В данное время нельзя назначить встречу");
                timePeriods.Add(oldMeeting.TimePeriod);
            }
        }

        private void ScheduleMeeting(Meeting meeting)
        {
            if (!СheckForIntersectionOfTimeIntervals(meeting.TimePeriod))
            {
                SetMeeting(meeting);
            }
            else Console.WriteLine("В данное время нельзя назначить встречу");
        }

        private void SetMeeting(Meeting meeting)
        {
            timePeriods.Add(meeting.TimePeriod);

            meetings.Add(meeting);

            IJobDetail jobDetail = CreateJob(meeting);

            ITrigger trigger = CreateTrigger(meeting);

            Scheduler.ScheduleJob(jobDetail, trigger).GetAwaiter();
        }

        private bool СheckForIntersectionOfTimeIntervals(TimeRange rangeTimeMeeting)
        {
            return timePeriods.HasIntersectionPeriods(rangeTimeMeeting);
        }

        private void DeleteMeeting(Meeting meeting)
        {
            Scheduler.DeleteJob(new JobKey(meeting.JobId.ToString()));
            meetings.Remove(meeting);
            timePeriods.Remove(meeting.TimePeriod);
        }

        private ITrigger CreateTrigger(Meeting meeting)
        {
            return TriggerBuilder.Create()
                .WithIdentity(meeting.JobId.ToString())
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(meeting.AlertTime.Hour, meeting.AlertTime.Minute))
                .WithDescription($"\n Оповещение о встречи c {meeting.TimePeriod.Start} до {meeting.TimePeriod.End}")
                .Build();
        }

        private IJobDetail CreateJob(Meeting meeting)
        {
            return JobBuilder.Create(typeof(NotificationJob))
                .WithIdentity(meeting.JobId.ToString())
                .Build();
        }

        private Meeting SetMeeting()
        {
            Console.WriteLine($"Введите дату начала встречи формата dd.mm.yyyy hh:mm");
            var timeStartMeeting = DateTime.Parse(Console.ReadLine());
            Console.WriteLine($"Введите дату окончания встречи формата dd.mm.yyyy hh:mm");
            var timeEndMeeting = DateTime.Parse(Console.ReadLine());
            Console.WriteLine($"Время оповещения формата dd.mm.yyyy hh:mm");
            var alertTime = DateTime.Parse(Console.ReadLine());
            if (ValidateMeeting(timeStartMeeting, timeEndMeeting, alertTime))
                return new Meeting(Guid.NewGuid(), new TimeRange(timeStartMeeting, timeEndMeeting), alertTime);
            throw new Exception("Неверно заданы данные о встрече");
        }

        private bool ValidateMeeting(DateTime timeStartMeeting, DateTime timeEndMeeting, DateTime alertTime)
        {
            return alertTime < timeStartMeeting && timeStartMeeting < timeEndMeeting;
        }

        private void Info()
        {
            Console.WriteLine("Планировщик встреч");
            Console.WriteLine("1 - Добавить встречу");
            Console.WriteLine("2 - Изменить встречу");
            Console.WriteLine("3 - Удалить встречу");
            Console.WriteLine("4 - Просмотреть встречи за опредленный день");
            Console.WriteLine("5 - Cохранить в файл");
        }
    }
}
