using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EchoBooster
{
    public class PerformanceScheduler
    {
        public class ScheduledTask
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string TaskType { get; set; } // "Optimize", "Cleanup", "Update", etc.
            public DateTime ScheduleTime { get; set; }
            public TimeSpan Recurrence { get; set; } // TimeSpan.Zero means no recurrence
            public bool Enabled { get; set; }
            public DateTime LastRun { get; set; }
            public string Status { get; set; }
            public object Parameters { get; set; }
        }

        private List<ScheduledTask> _scheduledTasks = new List<ScheduledTask>();
        private SystemBooster _booster;
        private SystemCleanup _cleanup;
        private UpdateManager _updateManager;

        public PerformanceScheduler(SystemBooster booster, SystemCleanup cleanup, UpdateManager updateManager)
        {
            _booster = booster;
            _cleanup = cleanup;
            _updateManager = updateManager;
        }

        public async Task<string> ScheduleTaskAsync(string name, string taskType, DateTime scheduleTime, TimeSpan recurrence, object parameters = null)
        {
            var task = new ScheduledTask
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                TaskType = taskType,
                ScheduleTime = scheduleTime,
                Recurrence = recurrence,
                Enabled = true,
                LastRun = DateTime.MinValue,
                Status = "Scheduled",
                Parameters = parameters
            };

            _scheduledTasks.Add(task);

            // Start monitoring for this task
            _ = Task.Run(() => MonitorTaskAsync(task));

            return task.Id;
        }

        public async Task<bool> CancelTaskAsync(string taskId)
        {
            var task = _scheduledTasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.Enabled = false;
                _scheduledTasks.Remove(task);
                return true;
            }
            return false;
        }

        public List<ScheduledTask> GetScheduledTasks()
        {
            return _scheduledTasks.ToList();
        }

        public async Task<List<ScheduledTask>> GetCompletedTasksAsync()
        {
            // In a real implementation, you would store completed tasks in a separate list or database
            // For now, return tasks that have run at least once
            return _scheduledTasks.Where(t => t.LastRun != DateTime.MinValue).ToList();
        }

        private async Task MonitorTaskAsync(ScheduledTask task)
        {
            while (task.Enabled && DateTime.Now < task.ScheduleTime.AddYears(1)) // Prevent infinite loop
            {
                await Task.Delay(30000); // Check every 30 seconds

                if (!task.Enabled) break;

                var now = DateTime.Now;
                if (now >= task.ScheduleTime && (task.LastRun == DateTime.MinValue || task.Recurrence != TimeSpan.Zero))
                {
                    await ExecuteTaskAsync(task);
                    
                    if (task.Recurrence != TimeSpan.Zero)
                    {
                        // Schedule next run
                        task.ScheduleTime = now.Add(task.Recurrence);
                    }
                    else
                    {
                        // One-time task, disable it
                        task.Enabled = false;
                    }
                }
            }
        }

        private async Task ExecuteTaskAsync(ScheduledTask task)
        {
            try
            {
                task.Status = "Running";
                
                switch (task.TaskType.ToLower())
                {
                    case "optimize":
                        await ExecuteOptimizeTaskAsync(task);
                        break;
                    case "cleanup":
                        await ExecuteCleanupTaskAsync(task);
                        break;
                    case "update":
                        await ExecuteUpdateTaskAsync(task);
                        break;
                    case "intelligentoptimize":
                        await ExecuteIntelligentOptimizeTaskAsync(task);
                        break;
                    default:
                        task.Status = "Unknown task type";
                        return;
                }

                task.LastRun = DateTime.Now;
                task.Status = "Completed";
            }
            catch (Exception ex)
            {
                task.Status = $"Failed: {ex.Message}";
            }
        }

        private async Task ExecuteOptimizeTaskAsync(ScheduledTask task)
        {
            await Task.Run(() => _booster.OptimizeProcesses());
        }

        private async Task ExecuteCleanupTaskAsync(ScheduledTask task)
        {
            var options = task.Parameters as SystemCleanup.CleanupOptions ?? new SystemCleanup.CleanupOptions();
            await _cleanup.PerformCleanupAsync(options);
        }

        private async Task ExecuteUpdateTaskAsync(ScheduledTask task)
        {
            await _updateManager.CheckForUpdatesAsync();
        }

        private async Task ExecuteIntelligentOptimizeTaskAsync(ScheduledTask task)
        {
            await Task.Run(() => _booster.IntelligentOptimize());
        }

        public async Task<string> ScheduleDailyOptimizationAsync(int hour, int minute)
        {
            var scheduleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, 0);
            if (scheduleTime <= DateTime.Now)
            {
                scheduleTime = scheduleTime.AddDays(1); // Schedule for tomorrow if time has passed today
            }

            return await ScheduleTaskAsync(
                "Daily Optimization",
                "optimize",
                scheduleTime,
                TimeSpan.FromDays(1)
            );
        }

        public async Task<string> ScheduleWeeklyCleanupAsync(DayOfWeek day, int hour, int minute)
        {
            var scheduleTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, minute, 0);
            
            // Adjust to the next occurrence of the specified day
            while (scheduleTime.DayOfWeek != day)
            {
                scheduleTime = scheduleTime.AddDays(1);
            }
            
            if (scheduleTime <= DateTime.Now)
            {
                scheduleTime = scheduleTime.AddDays(7); // Schedule for next week if date has passed
            }

            return await ScheduleTaskAsync(
                "Weekly Cleanup",
                "cleanup",
                scheduleTime,
                TimeSpan.FromDays(7),
                new SystemCleanup.CleanupOptions()
            );
        }

        public async Task<string> ScheduleIntelligentOptimizationAsync(TimeSpan interval)
        {
            var scheduleTime = DateTime.Now.Add(interval);
            
            return await ScheduleTaskAsync(
                "Intelligent Optimization",
                "intelligentoptimize",
                scheduleTime,
                interval
            );
        }

        public async Task<string> ScheduleSystemUpdateCheckAsync(TimeSpan interval)
        {
            var scheduleTime = DateTime.Now.Add(interval);
            
            return await ScheduleTaskAsync(
                "System Update Check",
                "update",
                scheduleTime,
                interval
            );
        }

        public async Task<bool> RunTaskNowAsync(string taskId)
        {
            var task = _scheduledTasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                await ExecuteTaskAsync(task);
                return true;
            }
            return false;
        }

        public async Task<bool> RescheduleTaskAsync(string taskId, DateTime newScheduleTime)
        {
            var task = _scheduledTasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.ScheduleTime = newScheduleTime;
                task.Status = "Rescheduled";
                return true;
            }
            return false;
        }

        public async Task ClearAllTasksAsync()
        {
            foreach (var task in _scheduledTasks.ToList())
            {
                task.Enabled = false;
            }
            _scheduledTasks.Clear();
        }

        public int GetActiveTaskCount()
        {
            return _scheduledTasks.Count(t => t.Enabled);
        }

        public async Task<List<ScheduledTask>> GetActiveTasksAsync()
        {
            return _scheduledTasks.Where(t => t.Enabled).ToList();
        }
    }
}