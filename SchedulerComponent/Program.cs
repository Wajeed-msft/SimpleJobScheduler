using System;

namespace SchedulerComponent
{
    class Program
    {
        static void Main(string[] args)
        {

            var scheduler = new Scheduler(new TimeSpan(0, 0, 5));

            var scheduleId1 = scheduler.AddSchedule(DateTime.UtcNow.AddSeconds(10),
                new Test() { Id = "10 seconds" }.Execute);

            var scheduleId2 = scheduler.AddSchedule(DateTime.UtcNow.AddSeconds(20),
                new Test() { Id = "20 seconds" }.Execute);

            //var scheduleId3 = scheduler.AddSchedule(DateTime.UtcNow.AddSeconds(30),
            //    new Test() { Id = "30 seconds" }.Execute);

            //var scheduleId4 = scheduler.AddSchedule(DateTime.UtcNow.AddSeconds(40),
            //    new Test() { Id = "40 seconds" }.Execute);

            //scheduler.RemoveSchedule(scheduleId4);

            var scheduleId5 = scheduler.AddSchedule(DateTime.UtcNow.AddSeconds(50),
                new Test() { Id = "50 seconds" }.Execute);

            //var scheduleId6 = scheduler.AddSchedule(DateTime.UtcNow.AddSeconds(1),
            //    new Test() { Id = "1 seconds" }.Execute);

            //var scheduleId7 = scheduler.AddSchedule(DateTime.UtcNow.AddSeconds(20),
            //    new Test() { Id = "35 seconds updated" }.Execute);

            //scheduler.UpdateSchedule(scheduleId7, DateTime.UtcNow.AddSeconds(35));

            System.Threading.Thread.Sleep(new TimeSpan(1, 0, 0));
            scheduler.Stop();
        }

    }
    class Test
    {
        public string Id { get; set; }
        public void Execute()
        {
            Console.WriteLine("Executing " + Id);
        }
    }
}
