using Itenso.TimePeriod;

namespace ServiceMeetings.Models
{
    public class Meeting
    {
        public Meeting()
        {

        }

        public Meeting(Guid jobId, TimeRange timePeriod, DateTime alertTime)
        {
            JobId = jobId;  
            TimePeriod = timePeriod;
            AlertTime = alertTime;
        }

        public Guid JobId { get; set; }

        public TimeRange TimePeriod { get; set; }

        public DateTime AlertTime { get; set; }

        public override string ToString()
        {
            return $"Время начала: {TimePeriod.Start}\nВремя окончания: {TimePeriod.End}";
        }
    }
}
