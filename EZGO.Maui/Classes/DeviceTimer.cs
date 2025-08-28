namespace EZGO.Maui.Classes.Timers
{
    public class DeviceTimer
    {
        /// <summary>
        /// Device timer let you start action with specified interval
        /// </summary>
        /// <param name="action">Action you want to execute every passed interval time</param>
        /// <param name="interval">Time that you want for the action to be executed, exp. every 30s</param>
        public DeviceTimer(Func<Task> action, TimeSpan interval)
        {
            Interval = interval;
            TimerAction = action;

            StartTimer();
        }

        public bool IsRunning { get; set; }
        public Func<Task> TimerAction { get; set; }
        public TimeSpan Interval { get; set; }

        private void StartTimer()
        {
            if (IsRunning) return;

            IsRunning = true;

            Start();
        }

        public void Stop()
        {
            IsRunning = false;
        }

        private void Start()
        {
            Application.Current?.Dispatcher.StartTimer(Interval, () =>
            {
                if (IsRunning)
                {
                    _ = TimerAction.Invoke(); // fire and forget without extra Task.Run
                    return true;
                }
                return false; // Stop the timer if IsRunning is false
            });
        }

        public void Restart()
        {
            Stop();

            if (TimerAction != null && Interval > TimeSpan.Zero)
            {
                StartTimer();
            }
        }
    }
}

