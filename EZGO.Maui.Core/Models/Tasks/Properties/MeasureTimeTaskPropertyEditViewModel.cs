using EZGO.Api.Models.PropertyValue;
using PropertyChanged;
using System;
using System.Timers;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace EZGO.Maui.Core.Models.Tasks.Properties
{
    /// <summary>
    /// Implementation of the <see cref="BaseTaskPropertyEditViewModel"/> that handles measuring time tesk feature
    /// </summary>
    public class MeasureTimeTaskPropertyEditViewModel : BaseTaskPropertyEditViewModel
    {
        #region Public Properties

        public string TimerLabelText { get; set; }

        public int? PopupTimeTaken { get; set; }

        public bool TimerStarted { get; set; }

        public bool TimerPaused { get; set; }

        [DoNotNotify]
        public bool TimerFinished { get; set; }

        #endregion

        #region Commands

        public ICommand StartTimerCommand => new Command(StartTimer);

        public ICommand PauseTimerCommand => new Command(PauseTimer);

        public ICommand StopTimerCommand => new Command(StopTimer);

        #endregion

        #region Private Members

        private const int secondsPerMinute = 60;

        private System.Timers.Timer timer;
        private int elapsedTimerSeconds;

        #endregion

        private void StartTimer()
        {
            timer.Start();

            TimerStarted = true;
        }

        private void PauseTimer()
        {
            if (!TimerPaused)
                timer.Stop();
            else
                timer.Start();

            TimerPaused = !TimerPaused;
        }

        private void StopTimer()
        {
            timer.Stop();

            int minutes = (elapsedTimerSeconds + secondsPerMinute - 1) / secondsPerMinute;

            PopupTimeTaken = minutes;

            TimerFinished = true;
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            timer?.Dispose();

            TimerStarted = false;
           TimerPaused = false;
            TimerLabelText = "00:00";

            elapsedTimerSeconds = 0;

            timer = new System.Timers.Timer
            {
                Interval = 1000
            };

            timer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            elapsedTimerSeconds++;

            TimeSpan time = TimeSpan.FromSeconds(elapsedTimerSeconds);
            TimerLabelText = time.ToString("mm\\:ss");
        }

        protected override void WriteChanges()
        {
            Value.UserValueTime = PopupTimeTaken.ToString();
        }

        protected override bool Validate()
        {
            var timerInProgress = TimerStarted && !TimerFinished;

            var inputCorrect = true;

            if (!PopupTimeTaken.HasValue || PopupTimeTaken.Value < 0)
                inputCorrect = false;

            return !timerInProgress && inputCorrect;
        }

        public MeasureTimeTaskPropertyEditViewModel(BasicTaskPropertyModel property) : base(property)
        {
            if (!string.IsNullOrEmpty(Value.UserValueTime) && int.TryParse(Value.UserValueTime, out var userTimeTaken))
                PopupTimeTaken = userTimeTaken;
            else if (int.TryParse(property.PrimaryValue, out var timeTaken))
                PopupTimeTaken = timeTaken;

            InitializeTimer();
        }
    }
}
