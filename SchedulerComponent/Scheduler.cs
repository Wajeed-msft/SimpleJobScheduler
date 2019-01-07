using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace SchedulerComponent
{

    public class Scheduler
    {
        private Timer mainScheduleTimer;
        private Dictionary<string, Timer> ScheduledTimers = new Dictionary<string, Timer>();
        private Dictionary<string, Schedule> ScheduleQueue { get; set; } = new Dictionary<string, Schedule>();
        private readonly TimeSpan _timespan;
        private readonly int offset = 10;// Milliseconds

        public Scheduler(TimeSpan interval)
        {
            _timespan = interval;
            mainScheduleTimer = new Timer(interval.TotalMilliseconds);
            // Hook up the Elapsed event for the timer.
            mainScheduleTimer.Elapsed += OnTimedEvent;
            mainScheduleTimer.AutoReset = true;
        }

        public string AddSchedule(DateTime dateTime, Action action)
        {
            var schedule = new Schedule()
            {
                Id = Guid.NewGuid().ToString(),
                ExecutionTime = dateTime,
                Action = action
            };
            ScheduleQueue.Add(schedule.Id, schedule);
            RefreshSchedules();
            return schedule.Id;
        }

        public bool UpdateSchedule(string scheduleId, DateTime updateDateTime)
        {
            if (!ScheduleQueue.ContainsKey(scheduleId))
            {
                return false;
            }

            ScheduleQueue[scheduleId].ExecutionTime = updateDateTime;

            if (ScheduledTimers.ContainsKey(scheduleId))
            {
                var diff = ScheduleQueue[scheduleId].ExecutionTime - DateTime.UtcNow;
                if (diff.TotalMilliseconds < (_timespan.TotalMilliseconds - offset))
                {
                    ScheduledTimers[scheduleId].Interval = diff.TotalMilliseconds > 0 ? diff.TotalMilliseconds : 1;
                }
            }
            CheckAndEnableScheduler();
            return true;
        }

        public bool RemoveSchedule(string scheduleId)
        {
            if (!ScheduleQueue.ContainsKey(scheduleId))
            {
                return false;
            }
            ScheduleQueue.Remove(scheduleId);
            ClenupScheduleTimer(scheduleId);
            return true;
        }

        private void CheckAndEnableScheduler()
        {
            mainScheduleTimer.Enabled = ScheduleQueue.Where(s => !s.Value.IsScheduled).Count() != 0;
            Console.WriteLine("Scheduler " + (mainScheduleTimer.Enabled? "Running" : "Stopped"));
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            RefreshSchedules();
        }

        private void RefreshSchedules()
        {
            var toSchedule = ScheduleQueue.Where(s => !s.Value.IsScheduled).OrderByDescending(s => s.Value.ExecutionTime);
            foreach (var schedule in toSchedule)
            {
                var diff = schedule.Value.ExecutionTime - DateTime.UtcNow;
                if (diff.TotalMilliseconds <= (_timespan.TotalMilliseconds - offset))// Just to be at safer side
                {
                    schedule.Value.IsScheduled = true;

                    // Create schedule
                    // scheduleTimers.Add();
                    var timer = new Timer();
                    timer.Elapsed += (sender, e) => OnTimedEvent(sender, e, schedule.Key);
                    ScheduledTimers.Add(schedule.Key, timer);
                    timer.AutoReset = false;
                    timer.Interval = diff.TotalMilliseconds > 0 ? diff.TotalMilliseconds : 1;
                    timer.Enabled = true;
                }
            }
            CheckAndEnableScheduler();
        }

        public void Stop()
        {
            mainScheduleTimer.Stop();
            mainScheduleTimer.Elapsed -= OnTimedEvent;
            mainScheduleTimer.Dispose();
        }

        void OnTimedEvent(object sender, ElapsedEventArgs e, string id)
        {
            ClenupScheduleTimer(id);
            if (ScheduleQueue.ContainsKey(id))
            {
                ScheduleQueue[id].Action();
                ScheduleQueue.Remove(id);
            }
        }

        private void ClenupScheduleTimer(string id)
        {
            if (ScheduledTimers.ContainsKey(id))
            {
                ScheduledTimers[id].Stop();
                // Clean the event
                ScheduledTimers[id].Elapsed -= (s, ex) => { };
                // Clean up the handlers
                ScheduledTimers[id].Dispose();
                ScheduledTimers[id] = null;
                ScheduledTimers.Remove(id);
            }
            CheckAndEnableScheduler();
        }
    }

    internal class Schedule
    {
        public DateTime ExecutionTime { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public Action Action { get; set; }
        internal bool IsScheduled { get; set; }
    }
}
